using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;
using Services;

namespace projetos.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FuncionariosController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly OficinaDbContext _context;
        private readonly IOficinaContext _oficinaContext;

        public FuncionariosController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, OficinaDbContext context, IOficinaContext oficinaContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _oficinaContext = oficinaContext;
        }

        public async Task<IActionResult> Index()
        {
            var oficina = await _oficinaContext.GetOficinaAtualAsync();
            if (oficina == null) throw new System.InvalidOperationException("Nenhuma oficina selecionada.");
            var grupoId = oficina.GrupoOficinaId;

            var usuariosIds = await _context.OficinasUsuarios
                .Where(ou => ou.Oficina.GrupoOficinaId == grupoId && (ou.Perfil == "Supervisor" || ou.Perfil == "Mecanico"))
                .Select(ou => ou.UsuarioId)
                .Distinct()
                .ToListAsync();

            var lista = await _userManager.Users
                .Where(u => usuariosIds.Contains(u.Id))
                .ToListAsync();

            return View(lista);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateFuncionarioViewModel());
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View(new ImportFuncionariosViewModel());
        }

        [HttpGet]
        public IActionResult ModeloImportacao()
        {
            var sb = new StringBuilder();
            sb.AppendLine("NomeCompleto;Email;Cargo;PercentualComissao;Senha");
            sb.AppendLine("João Mecânico;joao@exemplo.com;Mecanico;10;SenhaOpcional123");
            sb.AppendLine("Maria Supervisora;maria@exemplo.com;Supervisor;0;");

            using var ms = new MemoryStream();
            using (var writer = new StreamWriter(ms, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), leaveOpen: true))
            {
                writer.Write(sb.ToString());
            }
            ms.Position = 0;
            return File(ms.ToArray(), "text/csv", "modelo-funcionarios.csv");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateFuncionarioViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var cargoNormalizado = (model.Cargo ?? string.Empty).Trim();
            if (cargoNormalizado != "Supervisor" && cargoNormalizado != "Mecanico")
            {
                ModelState.AddModelError("Cargo", "Selecione Supervisor ou Mecanico.");
                return View(model);
            }
            if (model.PercentualComissao < 0)
            {
                ModelState.AddModelError(nameof(model.PercentualComissao), "Informe um percentual válido.");
                return View(model);
            }

            var exists = await _userManager.FindByEmailAsync(model.Email);
            if (exists != null)
            {
                ModelState.AddModelError("Email", "Já existe um usuário com este e-mail.");
                return View(model);
            }

            // Garante que a role exista
            if (!await _roleManager.RoleExistsAsync(cargoNormalizado))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(cargoNormalizado));
                if (!roleResult.Succeeded)
                {
                    foreach (var e in roleResult.Errors) ModelState.AddModelError(string.Empty, e.Description);
                    return View(model);
                }
            }

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                NomeCompleto = model.NomeCompleto,
                Cargo = cargoNormalizado,
                EmailConfirmed = true,
                PercentualComissao = model.PercentualComissao
            };
            var createRes = await _userManager.CreateAsync(user, string.IsNullOrWhiteSpace(model.Senha) ? "P@ssw0rd!" : model.Senha);
            if (!createRes.Succeeded)
            {
                foreach (var e in createRes.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }
            await _userManager.AddToRoleAsync(user, cargoNormalizado);

            var oficina = await _oficinaContext.GetOficinaAtualAsync();
            if (oficina == null) throw new System.InvalidOperationException("Nenhuma oficina selecionada.");

            _context.OficinasUsuarios.Add(new OficinaUsuario
            {
                UsuarioId = user.Id,
                OficinaId = oficina.Id,
                Perfil = cargoNormalizado,
                Ativo = true
            });
            await _context.SaveChangesAsync();

            TempData["Msg"] = $"Funcionário '{user.NomeCompleto}' criado como {cargoNormalizado}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(ImportFuncionariosViewModel model)
        {
            var oficina = await _oficinaContext.GetOficinaAtualAsync();
            if (oficina == null) throw new System.InvalidOperationException("Nenhuma oficina selecionada.");

            if (model.Arquivo == null || model.Arquivo.Length == 0)
            {
                ModelState.AddModelError(nameof(model.Arquivo), "Selecione um arquivo CSV preenchido.");
                return View(model);
            }

            var erros = new List<string>();
            var importados = 0;

            using var reader = new StreamReader(model.Arquivo.OpenReadStream(), Encoding.UTF8, true);
            string? line;
            var linhaNumero = 0;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                linhaNumero++;
                if (linhaNumero == 1 && line.Contains("NomeCompleto", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var campos = ParseCsvLine(line);
                if (campos.Length < 5)
                {
                    erros.Add($"Linha {linhaNumero}: formato inválido. Esperado 5 colunas.");
                    continue;
                }

                var nome = campos[0];
                var email = campos[1];
                var cargo = campos[2];
                var percentualTexto = campos[3];
                var senha = campos[4];

                if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(cargo))
                {
                    erros.Add($"Linha {linhaNumero}: nome, e-mail e cargo são obrigatórios.");
                    continue;
                }

                cargo = cargo.Trim();
                if (!string.Equals(cargo, "Supervisor", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(cargo, "Mecanico", StringComparison.OrdinalIgnoreCase))
                {
                    erros.Add($"Linha {linhaNumero}: cargo deve ser Supervisor ou Mecanico.");
                    continue;
                }
                var cargoNormalizado = char.ToUpperInvariant(cargo[0]) + cargo.Substring(1).ToLowerInvariant();

                if (!decimal.TryParse(percentualTexto, out var percentual) || percentual < 0)
                {
                    erros.Add($"Linha {linhaNumero}: percentual inválido.");
                    continue;
                }

                var existente = await _userManager.FindByEmailAsync(email);
                if (existente != null)
                {
                    if (!await UsuarioPertenceAoGrupoAsync(existente.Id, oficina.GrupoOficinaId))
                    {
                        erros.Add($"Linha {linhaNumero}: e-mail {email} já cadastrado em outro grupo.");
                        continue;
                    }

                    // já existe no grupo, apenas garante vínculo com oficina
                    await GarantirVinculoOficinaAsync(existente.Id, oficina.Id, cargoNormalizado);
                    importados++;
                    continue;
                }

                if (!await _roleManager.RoleExistsAsync(cargoNormalizado))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(cargoNormalizado));
                    if (!roleResult.Succeeded)
                    {
                        erros.Add($"Linha {linhaNumero}: não foi possível criar role {cargoNormalizado}.");
                        continue;
                    }
                }

                var usuario = new ApplicationUser
                {
                    Email = email.Trim(),
                    UserName = email.Trim(),
                    NomeCompleto = nome.Trim(),
                    Cargo = cargoNormalizado,
                    EmailConfirmed = true,
                    PercentualComissao = percentual
                };

                var senhaDefinida = string.IsNullOrWhiteSpace(senha) ? "P@ssw0rd!" : senha.Trim();
                var create = await _userManager.CreateAsync(usuario, senhaDefinida);
                if (!create.Succeeded)
                {
                    erros.Add($"Linha {linhaNumero}: {string.Join(", ", create.Errors.Select(e => e.Description))}");
                    continue;
                }

                await _userManager.AddToRoleAsync(usuario, cargoNormalizado);
                await GarantirVinculoOficinaAsync(usuario.Id, oficina.Id, cargoNormalizado);
                importados++;
            }

            model.Processado = true;
            model.TotalImportados = importados;
            model.Erros = erros;
            if (importados > 0)
            {
                TempData["Msg"] = $"{importados} funcionário(s) importado(s) com sucesso.";
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var oficina = await _oficinaContext.GetOficinaAtualAsync();
            if (oficina == null) throw new System.InvalidOperationException("Nenhuma oficina selecionada.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            if (!await UsuarioPertenceAoGrupoAsync(user.Id, oficina.GrupoOficinaId))
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var cargo = roles.Contains("Supervisor") ? "Supervisor" : (roles.Contains("Mecanico") ? "Mecanico" : (user.Cargo ?? string.Empty));
            var vm = new EditFuncionarioViewModel
            {
                Id = user.Id,
                NomeCompleto = user.NomeCompleto ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Cargo = cargo,
                Ativo = !(user.LockoutEnd.HasValue && user.LockoutEnd.Value.UtcDateTime > System.DateTime.UtcNow),
                PercentualComissao = user.PercentualComissao
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditFuncionarioViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var oficina = await _oficinaContext.GetOficinaAtualAsync();
            if (oficina == null) throw new System.InvalidOperationException("Nenhuma oficina selecionada.");

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();
            if (!await UsuarioPertenceAoGrupoAsync(user.Id, oficina.GrupoOficinaId))
            {
                return NotFound();
            }

            if (model.PercentualComissao < 0)
            {
                ModelState.AddModelError(nameof(model.PercentualComissao), "Informe um percentual válido.");
                return View(model);
            }

            user.NomeCompleto = model.NomeCompleto;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.Cargo = model.Cargo;
            user.PercentualComissao = model.PercentualComissao;

            var upd = await _userManager.UpdateAsync(user);
            if (!upd.Succeeded)
            {
                foreach (var e in upd.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            // Ajusta cargo (role)
            var desired = (model.Cargo ?? string.Empty).Trim();
            if (desired != "Supervisor" && desired != "Mecanico")
            {
                ModelState.AddModelError("Cargo", "Selecione Supervisor ou Mecanico.");
                return View(model);
            }
            if (!await _roleManager.RoleExistsAsync(desired))
            {
                await _roleManager.CreateAsync(new IdentityRole(desired));
            }
            var currentRoles = await _userManager.GetRolesAsync(user);
            var toRemove = currentRoles.Where(r => r == "Supervisor" || r == "Mecanico").ToList();
            if (toRemove.Any()) await _userManager.RemoveFromRolesAsync(user, toRemove);
            await _userManager.AddToRoleAsync(user, desired);

            // Ajusta status (ativo/inativo) via lockout
            var activeNow = !(user.LockoutEnd.HasValue && user.LockoutEnd.Value.UtcDateTime > System.DateTime.UtcNow);
            if (model.Ativo != activeNow)
            {
                if (!model.Ativo)
                {
                    user.LockoutEnabled = true;
                    user.LockoutEnd = System.DateTimeOffset.UtcNow.AddYears(100);
                }
                else
                {
                    user.LockoutEnd = null;
                }
                await _userManager.UpdateAsync(user);
            }

            // Troca de senha, se fornecida
            if (!string.IsNullOrWhiteSpace(model.NovaSenha))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var res = await _userManager.ResetPasswordAsync(user, token, model.NovaSenha);
                if (!res.Succeeded)
                {
                    foreach (var e in res.Errors) ModelState.AddModelError(string.Empty, e.Description);
                    return View(model);
                }
            }

            TempData["Msg"] = $"Funcionário '{user.NomeCompleto}' atualizado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAtivo(string id)
        {
            var oficina = await _oficinaContext.GetOficinaAtualAsync();
            if (oficina == null) throw new System.InvalidOperationException("Nenhuma oficina selecionada.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            if (!await UsuarioPertenceAoGrupoAsync(user.Id, oficina.GrupoOficinaId))
            {
                return NotFound();
            }

            var ativo = !(user.LockoutEnd.HasValue && user.LockoutEnd.Value.UtcDateTime > System.DateTime.UtcNow);
            if (ativo)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = System.DateTimeOffset.UtcNow.AddYears(100);
                TempData["Msg"] = $"Funcionário '{user.NomeCompleto}' desativado.";
            }
            else
            {
                user.LockoutEnd = null;
                TempData["Msg"] = $"Funcionário '{user.NomeCompleto}' reativado.";
            }
            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> UsuarioPertenceAoGrupoAsync(string usuarioId, int grupoId)
        {
            var grupos = await _context.OficinasUsuarios
                .Where(ou => ou.UsuarioId == usuarioId)
                .Select(ou => ou.Oficina.GrupoOficinaId)
                .Distinct()
                .ToListAsync();
            return grupos.Any(g => g == grupoId);
        }

        private async Task GarantirVinculoOficinaAsync(string usuarioId, int oficinaId, string perfil)
        {
            var vinculoExiste = await _context.OficinasUsuarios
                .AnyAsync(ou => ou.UsuarioId == usuarioId && ou.OficinaId == oficinaId);
            if (!vinculoExiste)
            {
                _context.OficinasUsuarios.Add(new OficinaUsuario
                {
                    UsuarioId = usuarioId,
                    OficinaId = oficinaId,
                    Perfil = perfil,
                    Ativo = true
                });
                await _context.SaveChangesAsync();
            }
        }

        private static string[] ParseCsvLine(string line)
        {
            var partes = line.Split(';');
            if (partes.Length == 1)
            {
                partes = line.Split(',');
            }
            return partes.Select(p => p?.Trim() ?? string.Empty).ToArray();
        }
    }
}
