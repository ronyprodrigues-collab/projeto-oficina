using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Infrastructure.Theming;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;

namespace projetos.Controllers
{
    [Authorize(Roles = "SuporteTecnico,Admin,Supervisor")]
    public class SuporteController : Controller
    {
        private readonly OficinaDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;

        public SuporteController(OficinaDbContext context, IWebHostEnvironment env, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? grupoId, int? oficinaId)
        {
            var grupos = await _context.Grupos
                .Include(g => g.Oficinas)
                .OrderBy(g => g.Nome)
                .ToListAsync();
            var defaults = await _context.Configuracoes.FirstOrDefaultAsync();

            if (!grupos.Any())
            {
                TempData["FixMsg"] = "Nenhum grupo cadastrado.";
                return View(new SuporteConfiguracaoViewModel { Defaults = defaults });
            }

            var grupoSelecionadoId = grupoId ?? grupos.First().Id;
            if (!grupos.Any(g => g.Id == grupoSelecionadoId))
            {
                grupoSelecionadoId = grupos.First().Id;
            }

            var grupoSelecionado = grupos.First(g => g.Id == grupoSelecionadoId);
            var oficinas = grupoSelecionado.Oficinas
                .OrderBy(o => o.Nome)
                .ToList();

            Oficina? oficinaSelecionada = null;
            int oficinaSelecionadaId = oficinaId ?? oficinas.FirstOrDefault()?.Id ?? 0;
            if (oficinaSelecionadaId > 0)
            {
                if (!oficinas.Any(o => o.Id == oficinaSelecionadaId))
                {
                    oficinaSelecionadaId = oficinas.FirstOrDefault()?.Id ?? 0;
                }

                if (oficinaSelecionadaId > 0)
                {
                    oficinaSelecionada = await _context.Oficinas
                        .Include(o => o.Grupo)
                        .FirstOrDefaultAsync(o => o.Id == oficinaSelecionadaId);
                }
            }

            var viewModel = new SuporteConfiguracaoViewModel
            {
                Grupos = grupos,
                GrupoSelecionadoId = grupoSelecionadoId,
                Oficinas = oficinas,
                OficinaSelecionadaId = oficinaSelecionadaId,
                Oficina = oficinaSelecionada,
                Defaults = defaults,
                GrupoSelecionado = grupoSelecionado,
                Paletas = ThemePalettes.All,
                TemaAtual = ThemePalettes.FromColors(grupoSelecionado.CorPrimaria, grupoSelecionado.CorSecundaria)
            };
            ViewData["ThemePrimaryOverride"] = viewModel.TemaAtual.PrimaryHex;
            ViewData["ThemeSecondaryOverride"] = viewModel.TemaAtual.SecondaryHex;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarLogo(int grupoId, int oficinaId, IFormFile? logo)
        {
            if (logo == null || logo.Length == 0)
                return RedirectToAction(nameof(Index), new { grupoId, oficinaId });

            var oficina = await _context.Oficinas.FirstOrDefaultAsync(o => o.Id == oficinaId);
            if (oficina == null)
                return RedirectToAction(nameof(Index), new { grupoId, oficinaId });

            var uploadsDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"logo_{System.DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(logo.FileName)}";
            var fullPath = Path.Combine(uploadsDir, fileName);
            using (var stream = System.IO.File.Create(fullPath))
            {
                await logo.CopyToAsync(stream);
            }
            var relativePath = $"/uploads/{fileName}";

            oficina.LogoPath = relativePath;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { grupoId, oficinaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarOficina(int grupoId, int oficinaId, string? nomeOficina)
        {
            var oficina = await _context.Oficinas.FirstOrDefaultAsync(o => o.Id == oficinaId);
            if (oficina == null)
                return RedirectToAction(nameof(Index), new { grupoId, oficinaId });

            if (!string.IsNullOrWhiteSpace(nomeOficina) && !string.Equals(oficina.Nome, nomeOficina, System.StringComparison.Ordinal))
            {
                oficina.Nome = nomeOficina!;
                await _context.SaveChangesAsync();
                TempData["FixMsg"] = "Nome da oficina atualizado.";
            }
            return RedirectToAction(nameof(Index), new { grupoId, oficinaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarTemaGrupo(int grupoId, int? oficinaId, string paletteKey)
        {
            if (!(User.IsInRole("SuporteTecnico") || User.IsInRole("Admin") || User.IsInRole("Diretor")))
            {
                return Forbid();
            }

            var grupo = await _context.Grupos
                .Include(g => g.Oficinas)
                .FirstOrDefaultAsync(g => g.Id == grupoId);
            if (grupo == null)
                return RedirectToAction(nameof(Index), new { grupoId, oficinaId });

            var palette = ThemePalettes.FromKey(paletteKey);
            grupo.CorPrimaria = palette.PrimaryHex;
            grupo.CorSecundaria = palette.SecondaryHex;
            foreach (var oficina in grupo.Oficinas)
            {
                oficina.CorPrimaria = palette.PrimaryHex;
                oficina.CorSecundaria = palette.SecondaryHex;
            }
            await _context.SaveChangesAsync();
            TempData["FixMsg"] = $"Tema do grupo atualizado para {palette.Name}.";
            return RedirectToAction(nameof(Index), new { grupoId, oficinaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarPlano(int grupoId, int oficinaId, PlanoConta plano)
        {
            var oficina = await _context.Oficinas.FirstOrDefaultAsync(o => o.Id == oficinaId);
            if (oficina == null)
                return RedirectToAction(nameof(Index), new { grupoId, oficinaId });

            oficina.Plano = plano;
            await _context.SaveChangesAsync();

            TempData["FixMsg"] = $"Plano da oficina atualizado para {plano}.";
            return RedirectToAction(nameof(Index), new { grupoId, oficinaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CorrigirValoresTeste()
        {
            // Corrige casos onde valores foram salvos 100x maiores (ex.: 50.00 -> 5000)
            var servicos = await _context.ServicoItem
                .Where(s => s.Valor >= 1000 && EF.Functions.Like(s.Valor.ToString(), "%00"))
                .ToListAsync();

            var pecas = await _context.PecaItem
                .Where(p => p.ValorUnitario >= 1000 && EF.Functions.Like(p.ValorUnitario.ToString(), "%00"))
                .ToListAsync();

            int sAjust = 0, pAjust = 0;
            foreach (var s in servicos)
            {
                var novo = Math.Round(s.Valor / 100m, 2);
                if (novo > 0 && novo < s.Valor) { s.Valor = novo; sAjust++; }
            }
            foreach (var p in pecas)
            {
                var novo = Math.Round(p.ValorUnitario / 100m, 2);
                if (novo > 0 && novo < p.ValorUnitario) { p.ValorUnitario = novo; pAjust++; }
            }
            if (sAjust + pAjust > 0)
                await _context.SaveChangesAsync();

            TempData["FixMsg"] = $"Corrigidos {sAjust} serviços e {pAjust} peças.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> UsuariosPendentes()
        {
            var pendentes = await (from u in _context.Users
                                   where !_context.OficinasUsuarios.Any(ou => ou.UsuarioId == u.Id)
                                         && !_context.Grupos.Any(g => g.DiretorId == u.Id)
                                   orderby string.IsNullOrWhiteSpace(u.NomeCompleto) ? u.Email : u.NomeCompleto
                                   select new { u.Id, u.NomeCompleto, u.Email, u.Cargo }).ToListAsync();

            var rolesLookup = await (from ur in _context.UserRoles
                                     join r in _context.Roles on ur.RoleId equals r.Id
                                     select new { ur.UserId, Role = r.Name }).ToListAsync();
            var perfisPorUsuario = rolesLookup
                .GroupBy(x => x.UserId)
                .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(x => x.Role)));

            var viewModel = pendentes.Select(p => new UsuarioPendenteViewModel
            {
                Id = p.Id,
                Nome = string.IsNullOrWhiteSpace(p.NomeCompleto) ? p.Email : p.NomeCompleto,
                Email = p.Email,
                Cargo = p.Cargo,
                Perfis = perfisPorUsuario.TryGetValue(p.Id, out var perfis) ? perfis : "-"
            }).ToList();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DefinirDiretor(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            if (!await _userManager.IsInRoleAsync(user, "Diretor"))
            {
                var result = await _userManager.AddToRoleAsync(user, "Diretor");
                if (!result.Succeeded)
                {
                    TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(UsuariosPendentes));
                }
            }
            user.Cargo = string.IsNullOrWhiteSpace(user.Cargo) ? "Diretor" : user.Cargo;
            await _userManager.UpdateAsync(user);
            TempData["FixMsg"] = $"Usuário '{user.NomeCompleto ?? user.Email}' agora possui perfil Diretor.";
            return RedirectToAction(nameof(UsuariosPendentes));
        }

        [HttpGet]
        public async Task<IActionResult> ReatribuirUsuario(string? usuarioId)
        {
            var vm = new ReatribuirUsuarioViewModel();
            await PopularReatribuicaoAsync(vm, usuarioId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReatribuirUsuario(ReatribuirUsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopularReatribuicaoAsync(model, model.UsuarioId);
                return View(model);
            }

            var user = string.IsNullOrWhiteSpace(model.UsuarioId)
                ? null
                : await _userManager.FindByIdAsync(model.UsuarioId);
            if (user == null)
            {
                ModelState.AddModelError(nameof(model.UsuarioId), "Usuário inválido.");
                await PopularReatribuicaoAsync(model, model.UsuarioId);
                return View(model);
            }

            var grupo = await _context.Grupos
                .Include(g => g.Oficinas)
                .FirstOrDefaultAsync(g => g.Id == model.GrupoId);
            if (grupo == null)
            {
                ModelState.AddModelError(nameof(model.GrupoId), "Grupo inválido.");
                await PopularReatribuicaoAsync(model, model.UsuarioId);
                return View(model);
            }

            var oficina = grupo.Oficinas.FirstOrDefault(o => o.Id == model.OficinaId);
            if (oficina == null)
            {
                ModelState.AddModelError(nameof(model.OficinaId), "Selecione uma oficina válida.");
                await PopularReatribuicaoAsync(model, model.UsuarioId);
                return View(model);
            }

            var vinculosExistentes = _context.OficinasUsuarios.Where(ou => ou.UsuarioId == user.Id);
            _context.OficinasUsuarios.RemoveRange(vinculosExistentes);

            _context.OficinasUsuarios.Add(new OficinaUsuario
            {
                OficinaId = oficina.Id,
                UsuarioId = user.Id,
                Perfil = model.Perfil,
                Ativo = true
            });

            if (model.Perfil == "Diretor")
            {
                grupo.DiretorId = user.Id;
            }

            if (!await _userManager.IsInRoleAsync(user, model.Perfil))
            {
                await _userManager.AddToRoleAsync(user, model.Perfil);
            }

            await _context.SaveChangesAsync();
            TempData["FixMsg"] = $"Usuário '{(user.NomeCompleto ?? user.Email)}' reatribuído para {oficina.Nome}.";
            return RedirectToAction(nameof(ReatribuirUsuario), new { usuarioId = user.Id });
        }

        private async Task PopularReatribuicaoAsync(ReatribuirUsuarioViewModel vm, string? usuarioId)
        {
            var usuarios = await _context.Users
                .OrderBy(u => string.IsNullOrWhiteSpace(u.NomeCompleto) ? u.Email : u.NomeCompleto)
                .Select(u => new
                {
                    u.Id,
                    Nome = string.IsNullOrWhiteSpace(u.NomeCompleto) ? u.Email : u.NomeCompleto
                })
                .ToListAsync();
            vm.Usuarios = usuarios
                .Select(u => new SelectListItem(u.Nome, u.Id, usuarioId == u.Id))
                .ToList();

            vm.Perfis = new List<SelectListItem>
            {
                new SelectListItem("Administrador", "Admin"),
                new SelectListItem("Diretor", "Diretor"),
                new SelectListItem("Supervisor", "Supervisor"),
                new SelectListItem("Mecanico", "Mecanico")
            };

            var grupos = await _context.Grupos
                .Include(g => g.Oficinas)
                .OrderBy(g => g.Nome)
                .ToListAsync();
            vm.Grupos = grupos.Select(g => new GrupoResumoViewModel
            {
                GrupoId = g.Id,
                GrupoNome = g.Nome,
                Oficinas = g.Oficinas.OrderBy(o => o.Nome).Select(o => new OficinaResumoViewModel
                {
                    OficinaId = o.Id,
                    Nome = o.Nome
                }).ToList()
            }).ToList();

            if (vm.GrupoId == 0 && vm.Grupos.Any())
            {
                vm.GrupoId = vm.Grupos.First().GrupoId;
            }

            if (vm.OficinaId == 0)
            {
                var grupoAtual = vm.Grupos.FirstOrDefault(g => g.GrupoId == vm.GrupoId);
                var primeiraOficina = grupoAtual?.Oficinas.FirstOrDefault();
                if (primeiraOficina != null)
                {
                    vm.OficinaId = primeiraOficina.OficinaId;
                }
            }
        }
    }
}
