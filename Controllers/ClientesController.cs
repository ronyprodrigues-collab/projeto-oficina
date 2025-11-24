using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Data;
using Models;
using Services;
using Models.ViewModels;

namespace projetos.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class ClientesController : Controller
    {
        private readonly OficinaDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOficinaContext _oficinaContext;

        public ClientesController(OficinaDbContext context, UserManager<ApplicationUser> userManager, IOficinaContext oficinaContext)
        {
            _context = context;
            _userManager = userManager;
            _oficinaContext = oficinaContext;
        }

        public async Task<IActionResult> Index()
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var clientes = await _context.Clientes
                .Where(c => c.Oficinas.Any(o => o.OficinaId == oficinaId))
                .Include(c => c.Veiculos)
                .Include(c => c.Oficinas)
                    .ThenInclude(oc => oc.Oficina)
                .AsNoTracking()
                .ToListAsync();
            return View(clientes);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var oficinaId = await ObterOficinaAtualIdAsync();
            var cliente = await _context.Clientes
                .Include(c => c.Veiculos)
                .FirstOrDefaultAsync(m => m.Id == id && m.Oficinas.Any(o => o.OficinaId == oficinaId));

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        public async Task<IActionResult> Create()
        {
            var responsavel = await ObterNomeResponsavelAsync();
            var model = new Cliente
            {
                Responsavel = responsavel
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View(new ImportClientesViewModel());
        }

        [HttpGet]
        public IActionResult ModeloImportacao()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Nome;Email;Telefone;CPF_CNPJ;TipoCliente;Responsavel;Endereco;Numero;Bairro;Cidade;Estado;CEP;Observacoes");
            sb.AppendLine("João Cliente;joao@cliente.com;(11)99999-1234;12345678901;PF;João;Rua das Flores;100;Centro;São Paulo;SP;01000-000;VIP");
            sb.AppendLine("Empresa Exemplo;contato@empresa.com;1133334444;12345678000199;PJ;Maria Responsável;Av. Paulista;2000;Bela Vista;São Paulo;SP;01311-000;Cliente corporativo");
            using var ms = new MemoryStream();
            using (var writer = new StreamWriter(ms, new UTF8Encoding(true), leaveOpen: true))
            {
                writer.Write(sb.ToString());
            }
            ms.Position = 0;
            return File(ms.ToArray(), "text/csv", "modelo-clientes.csv");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(
            "Id,Nome,CPF_CNPJ,Telefone,Email,Endereco," +
            "Numero,Bairro,Cidade,Estado,CEP," +
            "DataNascimento,Observacoes,TipoCliente,CNPJ,Responsavel"
        )] Cliente cliente)
        {
            if (string.IsNullOrWhiteSpace(cliente.Responsavel))
            {
                cliente.Responsavel = await ObterNomeResponsavelAsync();
            }

            if (ModelState.IsValid)
            {
                var oficinaId = await ObterOficinaAtualIdAsync();
                _context.Add(cliente);
                await _context.SaveChangesAsync();

                _context.OficinasClientes.Add(new OficinaCliente
                {
                    ClienteId = cliente.Id,
                    OficinaId = oficinaId,
                    VinculadoEm = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(ImportClientesViewModel model)
        {
            var oficina = await _oficinaContext.GetOficinaAtualAsync();
            if (oficina == null) throw new InvalidOperationException("Nenhuma oficina selecionada.");

            if (model.Arquivo == null || model.Arquivo.Length == 0)
            {
                ModelState.AddModelError(nameof(model.Arquivo), "Selecione um arquivo CSV válido.");
                return View(model);
            }

            var erros = new List<string>();
            var sucesso = 0;
            using var reader = new StreamReader(model.Arquivo.OpenReadStream(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            string? line;
            int linhaNum = 0;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                linhaNum++;
                if (linhaNum == 1 && line.IndexOf("Nome", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var campos = ParseCsvLine(line);
                if (campos.Length < 13)
                {
                    erros.Add($"Linha {linhaNum}: esperado 13 colunas.");
                    continue;
                }

                var nome = campos[0];
                var email = campos[1];
                var telefone = campos[2];
                var documento = campos[3];
                var tipo = campos[4];
                var responsavel = campos[5];
                var endereco = campos[6];
                var numero = campos[7];
                var bairro = campos[8];
                var cidade = campos[9];
                var estado = campos[10];
                var cep = campos[11];
                var observacoes = campos[12];

                var nomeNormalizado = string.IsNullOrWhiteSpace(nome) ? string.Empty : nome.Trim();
                var emailNormalizado = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
                var telefoneNormalizado = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
                var documentoNormalizado = string.IsNullOrWhiteSpace(documento) ? null : documento.Trim();
                var responsavelNormalizado = string.IsNullOrWhiteSpace(responsavel) ? null : responsavel.Trim();
                var enderecoNormalizado = string.IsNullOrWhiteSpace(endereco) ? null : endereco.Trim();
                var numeroNormalizado = string.IsNullOrWhiteSpace(numero) ? null : numero.Trim();
                var bairroNormalizado = string.IsNullOrWhiteSpace(bairro) ? null : bairro.Trim();
                var cidadeNormalizado = string.IsNullOrWhiteSpace(cidade) ? null : cidade.Trim();
                var estadoNormalizado = string.IsNullOrWhiteSpace(estado) ? null : estado.Trim();
                var cepNormalizado = string.IsNullOrWhiteSpace(cep) ? null : cep.Trim();
                var observacoesNormalizado = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim();

                if (string.IsNullOrWhiteSpace(nomeNormalizado))
                {
                    erros.Add($"Linha {linhaNum}: nome é obrigatório.");
                    continue;
                }

                if (emailNormalizado == null && documentoNormalizado == null)
                {
                    erros.Add($"Linha {linhaNum}: informe e-mail ou CPF/CNPJ para identificação.");
                    continue;
                }

                var tipoNormalizado = string.Equals(tipo, "PJ", StringComparison.OrdinalIgnoreCase) ? "PJ" : "PF";
                Cliente? existente = null;
                if (!string.IsNullOrWhiteSpace(documentoNormalizado))
                {
                    existente = await _context.Clientes
                        .Include(c => c.Oficinas)
                            .ThenInclude(oc => oc.Oficina)
                        .FirstOrDefaultAsync(c => c.CPF_CNPJ == documentoNormalizado);
                }
                if (existente == null && !string.IsNullOrWhiteSpace(emailNormalizado))
                {
                    existente = await _context.Clientes
                        .Include(c => c.Oficinas)
                            .ThenInclude(oc => oc.Oficina)
                        .FirstOrDefaultAsync(c => c.Email == emailNormalizado);
                }

                if (existente != null)
                {
                    if (ClientePertenceOutroGrupo(existente, oficina.GrupoOficinaId))
                    {
                        erros.Add($"Linha {linhaNum}: cliente já está vinculado a outro grupo.");
                        continue;
                    }

                    existente.Nome = string.IsNullOrWhiteSpace(existente.Nome) ? nomeNormalizado : existente.Nome;
                    if (!string.IsNullOrWhiteSpace(telefoneNormalizado))
                        existente.Telefone = telefoneNormalizado!;
                    if (!string.IsNullOrWhiteSpace(emailNormalizado))
                        existente.Email = emailNormalizado!;
                    if (string.IsNullOrWhiteSpace(existente.CPF_CNPJ) && documentoNormalizado != null)
                        existente.CPF_CNPJ = documentoNormalizado;
                    existente.TipoCliente = existente.TipoCliente ?? tipoNormalizado;
                    if (!string.IsNullOrWhiteSpace(responsavelNormalizado))
                        existente.Responsavel = responsavelNormalizado!;
                    if (!string.IsNullOrWhiteSpace(enderecoNormalizado))
                        existente.Endereco = enderecoNormalizado;
                    if (!string.IsNullOrWhiteSpace(numeroNormalizado))
                        existente.Numero = numeroNormalizado;
                    if (!string.IsNullOrWhiteSpace(bairroNormalizado))
                        existente.Bairro = bairroNormalizado;
                    if (!string.IsNullOrWhiteSpace(cidadeNormalizado))
                        existente.Cidade = cidadeNormalizado;
                    if (!string.IsNullOrWhiteSpace(estadoNormalizado))
                        existente.Estado = estadoNormalizado;
                    if (!string.IsNullOrWhiteSpace(cepNormalizado))
                        existente.CEP = cepNormalizado;
                    if (!string.IsNullOrWhiteSpace(observacoesNormalizado))
                    {
                        existente.Observacoes = string.IsNullOrWhiteSpace(existente.Observacoes)
                            ? observacoesNormalizado
                            : $"{existente.Observacoes} | {observacoesNormalizado}";
                    }

                    await VincularClienteAsync(existente.Id, oficina.Id);
                    sucesso++;
                    continue;
                }

                var cliente = new Cliente
                {
                    Nome = nomeNormalizado,
                    Email = emailNormalizado ?? string.Empty,
                    Telefone = telefoneNormalizado ?? string.Empty,
                    CPF_CNPJ = documentoNormalizado ?? string.Empty,
                    TipoCliente = tipoNormalizado,
                    Responsavel = responsavelNormalizado,
                    Endereco = enderecoNormalizado ?? string.Empty,
                    Numero = numeroNormalizado,
                    Bairro = bairroNormalizado,
                    Cidade = cidadeNormalizado,
                    Estado = estadoNormalizado,
                    CEP = cepNormalizado,
                    Observacoes = observacoesNormalizado
                };
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
                await VincularClienteAsync(cliente.Id, oficina.Id);
                sucesso++;
            }

            model.Processado = true;
            model.TotalImportados = sucesso;
            model.Erros = erros;
            if (sucesso > 0)
            {
                TempData["Msg"] = $"{sucesso} cliente(s) importado(s) com sucesso.";
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var oficinaId = await ObterOficinaAtualIdAsync();
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id && c.Oficinas.Any(o => o.OficinaId == oficinaId));

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(
            "Id,Nome,CPF_CNPJ,Telefone,Email,Endereco," +
            "Numero,Bairro,Cidade,Estado,CEP," +
            "DataNascimento,Observacoes,TipoCliente,CNPJ,Responsavel"
        )] Cliente cliente)
        {
            if (id != cliente.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var oficinaId = await ObterOficinaAtualIdAsync();
                    var original = await _context.Clientes
                        .FirstOrDefaultAsync(c => c.Id == id && c.Oficinas.Any(o => o.OficinaId == oficinaId));
                    if (original == null)
                    {
                        return NotFound();
                    }

                    _context.Entry(original).CurrentValues.SetValues(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(cliente);
        }

        private async Task<string> ObterNomeResponsavelAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return User?.Identity?.Name ?? string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(user.NomeCompleto))
            {
                return user.NomeCompleto;
            }

            return user.Email ?? user.UserName ?? string.Empty;
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var oficinaId = await ObterOficinaAtualIdAsync();
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.Id == id && m.Oficinas.Any(o => o.OficinaId == oficinaId));

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id && c.Oficinas.Any(o => o.OficinaId == oficinaId));

            if (cliente != null)
                _context.Clientes.Remove(cliente);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<int> ObterOficinaAtualIdAsync()
        {
            var oficina = await _oficinaContext.GetOficinaAtualAsync();
            if (oficina == null) throw new InvalidOperationException("Nenhuma oficina selecionada.");
            return oficina.Id;
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Vincular(string? busca)
        {
            var oficina = await _oficinaContext.GetOficinaAtualAsync();
            if (oficina == null) throw new InvalidOperationException("Nenhuma oficina selecionada.");
            var oficinaId = oficina.Id;
            var grupoId = oficina.GrupoOficinaId;

            var query = _context.Clientes
                .Where(c => !c.Oficinas.Any(o => o.OficinaId == oficinaId))
                .Where(c => !c.Oficinas.Any() || c.Oficinas.All(o => o.Oficina.GrupoOficinaId == grupoId));

            if (!string.IsNullOrWhiteSpace(busca))
            {
                query = query.Where(c =>
                    c.Nome.Contains(busca) ||
                    (c.CPF_CNPJ != null && c.CPF_CNPJ.Contains(busca)) ||
                    (c.CNPJ != null && c.CNPJ.Contains(busca)));
            }

            var candidatos = await query
                .Select(c => new
                {
                    c.Id,
                    c.Nome,
                    Documento = c.CNPJ ?? c.CPF_CNPJ ?? string.Empty,
                    Origem = c.Oficinas.Select(o => o.Oficina.Nome)
                })
                .OrderBy(c => c.Nome)
                .ToListAsync();

            var viewModel = new VincularClienteViewModel
            {
                Busca = busca,
                Clientes = candidatos
                    .Select(c => new ClienteDisponivelViewModel
                    {
                        Id = c.Id,
                        Nome = c.Nome,
                        Documento = c.Documento,
                        Origem = c.Origem.Any() ? string.Join(", ", c.Origem) : "Sem origem definida"
                    }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VincularCliente(int clienteId)
        {
            var oficina = await _oficinaContext.GetOficinaAtualAsync();
            if (oficina == null) throw new InvalidOperationException("Nenhuma oficina selecionada.");
            var oficinaId = oficina.Id;
            var grupoId = oficina.GrupoOficinaId;

            var existe = await _context.OficinasClientes
                .AnyAsync(oc => oc.OficinaId == oficinaId && oc.ClienteId == clienteId);
            if (existe)
            {
                TempData["Error"] = "Cliente já está disponível nesta oficina.";
                return RedirectToAction(nameof(Index));
            }

            var cliente = await _context.Clientes
                .Include(c => c.Oficinas)
                    .ThenInclude(oc => oc.Oficina)
                .FirstOrDefaultAsync(c => c.Id == clienteId);
            if (cliente == null)
            {
                TempData["Error"] = "Cliente não encontrado.";
                return RedirectToAction(nameof(Vincular));
            }

            var pertenceOutroGrupo = cliente.Oficinas
                .Any(oc => oc.Oficina.GrupoOficinaId != grupoId);
            if (pertenceOutroGrupo)
            {
                TempData["Error"] = "Este cliente já pertence a outro grupo de oficinas e não pode ser vinculado aqui.";
                return RedirectToAction(nameof(Vincular));
            }

            _context.OficinasClientes.Add(new OficinaCliente
            {
                ClienteId = clienteId,
                OficinaId = oficinaId,
                VinculadoEm = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["Msg"] = "Cliente vinculado à oficina.";
            return RedirectToAction(nameof(Index));
        }

        private bool ClientePertenceOutroGrupo(Cliente cliente, int grupoId)
        {
            return cliente.Oficinas.Any(oc => oc.Oficina.GrupoOficinaId != grupoId);
        }

        private async Task VincularClienteAsync(int clienteId, int oficinaId)
        {
            var jaVinculado = await _context.OficinasClientes.AnyAsync(oc => oc.OficinaId == oficinaId && oc.ClienteId == clienteId);
            if (!jaVinculado)
            {
                _context.OficinasClientes.Add(new OficinaCliente
                {
                    ClienteId = clienteId,
                    OficinaId = oficinaId,
                    VinculadoEm = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
        }

        private static string[] ParseCsvLine(string line)
        {
            var valores = line.Split(';');
            if (valores.Length == 1)
            {
                valores = line.Split(',');
            }
            return valores.Select(v => v?.Trim() ?? string.Empty).ToArray();
        }
    }
}
