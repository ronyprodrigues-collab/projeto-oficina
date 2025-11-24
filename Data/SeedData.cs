using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Models;

namespace Data
{
    public static class SeedData
    {
        public static async Task SeedRolesAndUsers(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var roles = new[] { "Admin", "Supervisor", "Mecanico", "SuporteTecnico", "Diretor" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));
                    if (!result.Succeeded)
                    {
                        Console.WriteLine($"Falha ao criar papel '{role}': {string.Join(", ", GetErrors(result))}");
                    }
                }
            }

            // Defina credenciais para o primeiro acesso (podem vir por variáveis de ambiente ou appsettings)
            var defaultPassword = Environment.GetEnvironmentVariable("SEED_DEFAULT_PASSWORD")
                                    ?? configuration["InitialAdmin:DefaultPassword"]
                                    ?? "P@ssw0rd!";
var adminEmail = Environment.GetEnvironmentVariable("SEED_ADMIN_EMAIL")
                               ?? configuration["InitialAdmin:Email"]
                               ?? "admin@oficina.local";
            var adminName = Environment.GetEnvironmentVariable("SEED_ADMIN_NAME")
                              ?? configuration["InitialAdmin:Name"]
                              ?? "Administrador";
            var adminPassword = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD")
                                   ?? configuration["InitialAdmin:Password"]
                                   ?? defaultPassword;
            // Leitura de arquivo inicial do instalador (initial-admin.json) somente no primeiro start
            try
            {
                var installerFile = System.IO.Path.Combine(AppContext.BaseDirectory, "initial-admin.json");
                if (System.IO.File.Exists(installerFile))
                {
                    using var jdoc = System.Text.Json.JsonDocument.Parse(System.IO.File.ReadAllText(installerFile));
                    var root = jdoc.RootElement;
                    if (root.TryGetProperty("InitialAdmin", out var ia)) root = ia;
                    if (root.TryGetProperty("Email", out var je)) adminEmail = je.GetString() ?? adminEmail;
                    if (root.TryGetProperty("Name", out var jn)) adminName = jn.GetString() ?? adminName;
                    if (root.TryGetProperty("Password", out var jp)) adminPassword = jp.GetString() ?? adminPassword;
                    try { System.IO.File.Delete(installerFile); } catch {}
                }
            }
            catch { }

            var usersToSeed = new (string Email, string Role, string Nome, string Cargo, string? Password)[]
            {
                (adminEmail,                  "Admin",          adminName,        "Admin",          adminPassword),
                ("supervisor@oficina.local", "Supervisor",      "Supervisor",    "Supervisor",     null),
                ("mecanico@oficina.local",   "Mecanico",        "Mecânico",      "Mecanico",       null),
                ("suporte@oficina.local",    "SuporteTecnico",  "Suporte Técnico","SuporteTecnico", null),
                ("diretor@oficina.local",    "Diretor",         "Diretor Geral", "Diretor",        null),
            };

            foreach (var (email, role, nome, cargo, specificPassword) in usersToSeed)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        NomeCompleto = nome,
                        Cargo = cargo,
                        PercentualComissao = cargo == "Mecanico" ? 10 : 0
                    };
                    var createResult = await userManager.CreateAsync(user, specificPassword ?? defaultPassword);
                    if (!createResult.Succeeded)
                    {
                        Console.WriteLine($"Falha ao criar usuário '{email}': {string.Join(", ", GetErrors(createResult))}");
                        continue;
                    }
                }

                if (!await userManager.IsInRoleAsync(user, role))
                {
                    var addRoleResult = await userManager.AddToRoleAsync(user, role);
                    if (!addRoleResult.Succeeded)
                    {
                        Console.WriteLine($"Falha ao atribuir papel '{role}' ao usuário '{email}': {string.Join(", ", GetErrors(addRoleResult))}");
                    }
                }
                else if (role == "Mecanico" && user.PercentualComissao <= 0)
                {
                    user.PercentualComissao = 10;
                    await userManager.UpdateAsync(user);
                }
            }
        }

        public static async Task SeedDemoData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();

            var diretorPadrao = await db.Users.FirstOrDefaultAsync(u => u.Email == "diretor@oficina.local");
            if (diretorPadrao == null)
            {
                diretorPadrao = await db.Users.OrderBy(u => u.Id).FirstOrDefaultAsync();
            }

            var grupoPadrao = await db.Grupos.IgnoreQueryFilters().FirstOrDefaultAsync(g => g.Nome == "Grupo Demo");
            if (grupoPadrao == null)
            {
                grupoPadrao = new GrupoOficina
                {
                    Nome = "Grupo Demo",
                    Descricao = "Grupo padrão para demonstração",
                    DiretorId = diretorPadrao?.Id ?? string.Empty,
                    Plano = PlanoConta.Plus,
                    CorPrimaria = "#0d6efd",
                    CorSecundaria = "#6c757d"
                };
                db.Grupos.Add(grupoPadrao);
                await db.SaveChangesAsync();
            }
            else if (string.IsNullOrWhiteSpace(grupoPadrao.DiretorId) && diretorPadrao != null)
            {
                grupoPadrao.DiretorId = diretorPadrao.Id;
                await db.SaveChangesAsync();
            }

            var oficinaPadrao = await db.Oficinas.IgnoreQueryFilters().FirstOrDefaultAsync(o => o.Nome == "Oficina Principal");
            if (oficinaPadrao == null)
            {
                oficinaPadrao = new Oficina
                {
                    Nome = "Oficina Principal",
                    Descricao = "Oficina inicial",
                    GrupoOficinaId = grupoPadrao.Id,
                    Plano = grupoPadrao.Plano,
                    CorPrimaria = "#0d6efd",
                    CorSecundaria = "#6c757d",
                    FinanceiroPrazoSemJurosDias = 90,
                    FinanceiroJurosMensal = 0.02m
                };
                db.Oficinas.Add(oficinaPadrao);
                await db.SaveChangesAsync();
            }
            else
            {
                var atualizado = false;
                if (oficinaPadrao.FinanceiroPrazoSemJurosDias <= 0)
                {
                    oficinaPadrao.FinanceiroPrazoSemJurosDias = 90;
                    atualizado = true;
                }
                if (oficinaPadrao.FinanceiroJurosMensal <= 0)
                {
                    oficinaPadrao.FinanceiroJurosMensal = 0.02m;
                    atualizado = true;
                }
                if (atualizado)
                {
                    await db.SaveChangesAsync();
                }
            }

            if (!await db.ContasFinanceiras.AnyAsync(c => c.OficinaId == oficinaPadrao.Id))
            {
                db.ContasFinanceiras.AddRange(
                    new ContaFinanceira
                    {
                        OficinaId = oficinaPadrao.Id,
                        Nome = "Caixa Principal",
                        Tipo = FinanceiroTipoConta.Caixa,
                        SaldoInicial = 0
                    },
                    new ContaFinanceira
                    {
                        OficinaId = oficinaPadrao.Id,
                        Nome = "Banco Padrão",
                        Tipo = FinanceiroTipoConta.Banco,
                        Banco = "000",
                        Agencia = "0000",
                        NumeroConta = "000000-0",
                        SaldoInicial = 0
                    });
                await db.SaveChangesAsync();
            }

            if (!await db.CategoriasFinanceiras.AnyAsync(c => c.OficinaId == oficinaPadrao.Id))
            {
                db.CategoriasFinanceiras.AddRange(
                    new CategoriaFinanceira
                    {
                        OficinaId = oficinaPadrao.Id,
                        Nome = "Serviços de Oficina",
                        Tipo = FinanceiroTipoLancamento.Receita,
                        Descricao = "Receitas provenientes de ordens de serviço."
                    },
                    new CategoriaFinanceira
                    {
                        OficinaId = oficinaPadrao.Id,
                        Nome = "Compra de Peças",
                        Tipo = FinanceiroTipoLancamento.Despesa,
                        Descricao = "Reposição de estoque e compra de insumos."
                    },
                    new CategoriaFinanceira
                    {
                        OficinaId = oficinaPadrao.Id,
                        Nome = "Folha de Pagamento",
                        Tipo = FinanceiroTipoLancamento.Despesa,
                        Descricao = "Custos com salários e encargos."
                    });
                await db.SaveChangesAsync();
            }

            var usuariosExistentes = await db.Users.ToListAsync();
            foreach (var usuario in usuariosExistentes)
            {
                if (!await db.OficinasUsuarios.AnyAsync(ou => ou.OficinaId == oficinaPadrao.Id && ou.UsuarioId == usuario.Id))
                {
                    db.OficinasUsuarios.Add(new OficinaUsuario
                    {
                        OficinaId = oficinaPadrao.Id,
                        UsuarioId = usuario.Id,
                        Perfil = string.IsNullOrWhiteSpace(usuario.Cargo) ? "Colaborador" : usuario.Cargo,
                        VinculadoEm = DateTime.UtcNow
                    });
                }
            }
            await db.SaveChangesAsync();

            if (await db.Clientes.AnyAsync())
            {
                if (!await db.Configuracoes.AnyAsync())
                {
                    db.Configuracoes.Add(new Configuracoes
                    {
                        NomeOficina = "Oficina Modelo",
                        CorPrimaria = "#0d6efd",
                        CorSecundaria = "#6c757d",
                        LogoPath = null,
                        PlanoAtual = PlanoConta.Pro
                    });
                    await db.SaveChangesAsync();
                }

                return;
            }

            if (!await db.Configuracoes.AnyAsync())
            {
                db.Configuracoes.Add(new Configuracoes
                {
                    NomeOficina = "Oficina Modelo",
                    CorPrimaria = "#0d6efd",
                    CorSecundaria = "#6c757d",
                    LogoPath = null,
                    PlanoAtual = PlanoConta.Pro
                });
            }

            var responsavelDefault = await db.Users
                .Select(u => u.NomeCompleto)
                .FirstOrDefaultAsync(n => !string.IsNullOrWhiteSpace(n))
                ?? "Administrador";

            var rnd = new Random(12345);

            string[] firstNames = { "Ronaldo", "Maria", "João", "Ana", "Carlos", "Patrícia", "Marcelo", "Juliana", "Marcos", "Luciana", "Pedro", "Camila" };
            string[] lastNames = { "Silva", "Souza", "Oliveira", "Santos", "Rodrigues", "Almeida", "Lima", "Gomes", "Barbosa", "Rocha" };
            string[] streets = { "Av. Brasil", "Rua das Flores", "Rua Projetada", "Av. Central", "Rua das Palmeiras", "Rua Rio Negro", "Rua Sete" };
            string[] cities = { "São Paulo", "Campinas", "Guarulhos", "Santos", "Sorocaba", "Osasco" };
            string[] states = { "SP", "RJ", "MG", "PR" };
            string[] brands = { "Volkswagen", "Chevrolet", "Ford", "Fiat", "Hyundai", "Toyota", "Honda", "Mercedes-Benz" };
            string[] models = { "Gol", "Onix", "Ka", "Argo", "HB20", "Corolla", "Civic", "C-180" };
            string[] servicosDesc = { "Troca de óleo", "Alinhamento e balanceamento", "Revisão básica", "Troca de pastilhas", "Limpeza de bicos", "Troca de filtro de ar", "Troca de bateria" };
            string[] motivosReprovacao = {
                "Cliente considerou o orçamento acima do esperado.",
                "Veículo foi levado para outra oficina.",
                "Cliente preferiu aguardar peças genéricas.",
                "Cliente solicitou novos itens antes de aprovar."
            };

            string RandomDigits(int length)
            {
                var chars = new char[length];
                for (int i = 0; i < length; i++)
                {
                    chars[i] = (char)('0' + rnd.Next(0, 10));
                }
                return new string(chars);
            }

            string RandomCpf() => RandomDigits(11);
            string RandomCnpj() => RandomDigits(14);
            string RandomCep() => RandomDigits(8);

            string RandomPhone()
            {
                var ddds = new[] { 11, 12, 13, 19, 21, 31, 41 };
                var ddd = ddds[rnd.Next(ddds.Length)];
                return $"({ddd}) 9{rnd.Next(4000, 9999)}-{rnd.Next(1000, 9999)}";
            }

            string RandomPlate()
            {
                const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                return new string(new[]
                {
                    letters[rnd.Next(letters.Length)],
                    letters[rnd.Next(letters.Length)],
                    letters[rnd.Next(letters.Length)]
                }) + "-" + rnd.Next(1000, 9999);
            }

            DateTime RandomDate(int daysBack)
            {
                return DateTime.UtcNow.AddDays(-rnd.Next(daysBack)).Date.AddHours(rnd.Next(8, 18));
            }

            // Peças de estoque e movimentações iniciais
            var pecasCatalogo = new[]
            {
                new { Nome = "Óleo 5W30", Codigo = "OL-5W30", Unidade = "L", Minimo = 10m, Venda = 95m },
                new { Nome = "Filtro de óleo", Codigo = "FIL-OL", Unidade = "pc", Minimo = 15m, Venda = 45m },
                new { Nome = "Pastilha de freio", Codigo = "PAST-FR", Unidade = "jogo", Minimo = 8m, Venda = 180m },
                new { Nome = "Bateria 60Ah", Codigo = "BAT-60", Unidade = "pc", Minimo = 5m, Venda = 520m },
                new { Nome = "Velas de ignição", Codigo = "VELA-IG", Unidade = "pc", Minimo = 20m, Venda = 35m },
                new { Nome = "Aditivo de radiador", Codigo = "AD-RAD", Unidade = "L", Minimo = 12m, Venda = 60m }
            };

            var pecasEstoque = new List<PecaEstoque>();
            foreach (var item in pecasCatalogo)
            {
                var peca = new PecaEstoque
                {
                    Nome = item.Nome,
                    Codigo = item.Codigo,
                    UnidadeMedida = item.Unidade,
                    EstoqueMinimo = item.Minimo,
                    PrecoVenda = item.Venda,
                    SaldoAtual = 0,
                    OficinaId = oficinaPadrao.Id
                };
                db.PecaEstoques.Add(peca);
                pecasEstoque.Add(peca);
            }

            await db.SaveChangesAsync();

            foreach (var peca in pecasEstoque)
            {
                var quantidade = (decimal)rnd.Next(25, 80);
                var custo = Math.Round((peca.PrecoVenda ?? 50m) * (decimal)(0.55 + rnd.NextDouble() * 0.25), 2);
                db.MovimentacoesEstoque.Add(new MovimentacaoEstoque
                {
                    PecaEstoqueId = peca.Id,
                    Tipo = "Entrada",
                    Quantidade = quantidade,
                    QuantidadeRestante = quantidade,
                    ValorUnitario = custo,
                    Observacao = "Carga inicial automática",
                    DataMovimentacao = DateTime.UtcNow.AddDays(-rnd.Next(45)),
                    OficinaId = oficinaPadrao.Id
                });
                peca.SaldoAtual += quantidade;
            }

            await db.SaveChangesAsync();

            // Clientes e veículos
            var clientes = new List<Cliente>();
            var veiculos = new List<Veiculo>();
            for (int i = 0; i < 10; i++)
            {
                var nomeBase = $"{firstNames[rnd.Next(firstNames.Length)]} {lastNames[rnd.Next(lastNames.Length)]}";
                var isPessoaJuridica = i % 4 == 0;
                var cidade = cities[rnd.Next(cities.Length)];
                var estado = states[rnd.Next(states.Length)];
                var slug = nomeBase.ToLower().Replace(' ', '.');

                var cliente = new Cliente
                {
                    Nome = isPessoaJuridica ? $"{lastNames[rnd.Next(lastNames.Length)]} Serviços LTDA" : nomeBase,
                    CPF_CNPJ = isPessoaJuridica ? string.Empty : RandomCpf(),
                    CNPJ = isPessoaJuridica ? RandomCnpj() : null,
                    TipoCliente = isPessoaJuridica ? "PJ" : "PF",
                    Telefone = RandomPhone(),
                    Email = $"{slug}{i}@demo.com",
                    Endereco = streets[rnd.Next(streets.Length)],
                    Numero = rnd.Next(10, 999).ToString(),
                    Bairro = $"Bairro {rnd.Next(1, 20)}",
                    Cidade = cidade,
                    Estado = estado,
                    CEP = RandomCep(),
                    DataNascimento = isPessoaJuridica ? null : DateTime.UtcNow.AddYears(-20 - rnd.Next(25)).Date,
                    Observacoes = rnd.NextDouble() < 0.3 ? "Cliente importado automaticamente." : null,
                    Responsavel = responsavelDefault
                };

                db.Clientes.Add(cliente);
                clientes.Add(cliente);
                db.OficinasClientes.Add(new OficinaCliente
                {
                    Cliente = cliente,
                    OficinaId = oficinaPadrao.Id,
                    VinculadoEm = DateTime.UtcNow
                });

                int vcount = 1 + rnd.Next(3);
                for (int v = 0; v < vcount; v++)
                {
                    var marca = brands[rnd.Next(brands.Length)];
                    var modelo = models[rnd.Next(models.Length)];
                    var veiculo = new Veiculo
                    {
                        Cliente = cliente,
                        Placa = RandomPlate(),
                        Marca = marca,
                        Modelo = modelo,
                        Ano = rnd.Next(2008, DateTime.UtcNow.Year)
                    };
                    db.Veiculos.Add(veiculo);
                    veiculos.Add(veiculo);
                    db.OficinasVeiculos.Add(new OficinaVeiculo
                    {
                        Veiculo = veiculo,
                        OficinaId = oficinaPadrao.Id,
                        VinculadoEm = DateTime.UtcNow
                    });
                }
            }

            await db.SaveChangesAsync();

            // Mecânicos disponíveis
            var mecanicos = new List<ApplicationUser>();
            var mecRoleId = await db.Roles.Where(r => r.Name == "Mecanico").Select(r => r.Id).FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(mecRoleId))
            {
                mecanicos = await (from u in db.Users
                                   join ur in db.UserRoles on u.Id equals ur.UserId
                                   where ur.RoleId == mecRoleId
                                   select u).ToListAsync();
            }

            // Ordens de serviço
            for (int i = 0; i < 40; i++)
            {
                var cliente = clientes[rnd.Next(clientes.Count)];
                var veiculosDoCliente = veiculos.Where(v => v.ClienteId == cliente.Id).ToList();
                if (!veiculosDoCliente.Any())
                {
                    continue;
                }

                var veiculo = veiculosDoCliente[rnd.Next(veiculosDoCliente.Count)];
                var abertura = RandomDate(120);
                var prevista = abertura.AddDays(rnd.Next(1, 12));
                var concluida = rnd.NextDouble() < 0.45;
                var aprovada = concluida || rnd.NextDouble() < 0.65;
                var reprovada = !aprovada && rnd.NextDouble() < 0.4;
                var mecanico = (!reprovada && mecanicos.Count > 0) ? mecanicos[rnd.Next(mecanicos.Count)] : null;

                var status = reprovada
                    ? "Reprovada"
                    : concluida
                        ? "Concluida"
                        : aprovada
                            ? "Em execução"
                            : "Aguardando aprovação";

                var ordem = new OrdemServico
                {
                    ClienteId = cliente.Id,
                    VeiculoId = veiculo.Id,
                    MecanicoId = mecanico?.Id,
                    OficinaId = oficinaPadrao.Id,
                    Descricao = servicosDesc[rnd.Next(servicosDesc.Length)],
                    DataAbertura = abertura,
                    DataPrevista = prevista,
                    DataConclusao = concluida ? prevista.AddDays(rnd.Next(0, 3)) : (DateTime?)null,
                    Status = status,
                    AprovadaCliente = reprovada ? false : aprovada,
                    MotivoReprovacao = reprovada ? motivosReprovacao[rnd.Next(motivosReprovacao.Length)] : null,
                    Observacoes = rnd.NextDouble() < 0.25 ? "Gerada automaticamente para demonstração." : null
                };

                int svcCount = 1 + rnd.Next(3);
                for (int s = 0; s < svcCount; s++)
                {
                    ordem.Servicos.Add(new ServicoItem
                    {
                        Descricao = servicosDesc[rnd.Next(servicosDesc.Length)],
                        Valor = Math.Round((decimal)(rnd.NextDouble() * 700 + 120), 2),
                        Concluido = concluida || rnd.NextDouble() < 0.5
                    });
                }

                int pecCount = rnd.Next(0, 3);
                bool reservouEstoque = false;
                for (int p = 0; p < pecCount && pecasEstoque.Count > 0; p++)
                {
                    var peca = pecasEstoque[rnd.Next(pecasEstoque.Count)];
                    var quantidade = 1 + rnd.Next(3);
                    ordem.Pecas.Add(new PecaItem
                    {
                        Nome = peca.Nome,
                        ValorUnitario = peca.PrecoVenda ?? 0m,
                        Quantidade = quantidade,
                        PecaEstoqueId = peca.Id,
                        Concluido = concluida || rnd.NextDouble() < 0.5
                    });
                    reservouEstoque = true;
                }

                ordem.EstoqueReservado = reservouEstoque && !reprovada;

                db.OrdensServico.Add(ordem);
                await db.SaveChangesAsync();
            }
        }

        private static IEnumerable<string> GetErrors(IdentityResult result)
        {
            foreach (var e in result.Errors)
            {
                yield return $"{e.Code}:{e.Description}";
            }
        }
    }
}
