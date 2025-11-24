using System.Linq;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;

namespace projetos.Controllers
{
    [Authorize(Roles = "Diretor")]
    public class DiretorController : Controller
    {
        private readonly OficinaDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DiretorController(OficinaDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var grupos = await _context.Grupos
                .Where(g => g.DiretorId == user.Id)
                .Include(g => g.Oficinas)
                .ToListAsync();

            return View(grupos);
        }

        public async Task<IActionResult> DetalhesGrupo(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var grupo = await _context.Grupos
                .Include(g => g.Oficinas)
                    .ThenInclude(o => o.AdminProprietario)
                .Include(g => g.Administrador)
                .FirstOrDefaultAsync(g => g.Id == id && g.DiretorId == user.Id);

            if (grupo == null) return NotFound();

            await PopularAdminsAsync(grupo.AdministradorId);
            return View(grupo);
        }

        public async Task<IActionResult> CriarAdministrador(int grupoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var grupo = await _context.Grupos.FirstOrDefaultAsync(g => g.Id == grupoId && g.DiretorId == user.Id);
            if (grupo == null) return NotFound();

            var vm = new CriarAdministradorViewModel
            {
                GrupoId = grupo.Id,
                GrupoNome = grupo.Nome
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarAdministrador(CriarAdministradorViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var grupo = await _context.Grupos.FirstOrDefaultAsync(g => g.Id == model.GrupoId && g.DiretorId == user.Id);
            if (grupo == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.GrupoNome = grupo.Nome;
                return View(model);
            }

            var email = model.Email.Trim();
            var novoAdmin = new ApplicationUser
            {
                Email = email,
                UserName = email,
                NomeCompleto = model.NomeCompleto?.Trim() ?? string.Empty,
                Cargo = string.IsNullOrWhiteSpace(model.Cargo) ? "Administrador" : model.Cargo.Trim(),
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(novoAdmin, model.Senha);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                model.GrupoNome = grupo.Nome;
                return View(model);
            }

            await _userManager.AddToRoleAsync(novoAdmin, "Admin");

            grupo.AdministradorId = novoAdmin.Id;
            await _context.SaveChangesAsync();

            TempData["Msg"] = $"Administrador '{ObterNomeExibicao(novoAdmin)}' criado para o grupo.";
            return RedirectToAction(nameof(DetalhesGrupo), new { id = grupo.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DefinirAdministrador(int grupoId, string? adminId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var grupo = await _context.Grupos.FirstOrDefaultAsync(g => g.Id == grupoId && g.DiretorId == user.Id);
            if (grupo == null) return NotFound();

            ApplicationUser? adminSelecionado = null;
            if (!string.IsNullOrEmpty(adminId))
            {
                adminSelecionado = await _userManager.FindByIdAsync(adminId);
                if (adminSelecionado == null || !await _userManager.IsInRoleAsync(adminSelecionado, "Admin"))
                {
                    TempData["Error"] = "Administrador inválido.";
                    return RedirectToAction(nameof(DetalhesGrupo), new { id = grupo.Id });
                }
            }

            grupo.AdministradorId = adminSelecionado?.Id;
            await _context.SaveChangesAsync();

            TempData["Msg"] = adminSelecionado == null
                ? "Administrador removido do grupo."
                : $"Administrador {ObterNomeExibicao(adminSelecionado)} vinculado ao grupo.";

            return RedirectToAction(nameof(DetalhesGrupo), new { id = grupo.Id });
        }

        public async Task<IActionResult> CriarOficina(int grupoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var grupo = await _context.Grupos.FirstOrDefaultAsync(g => g.Id == grupoId && g.DiretorId == user.Id);
            if (grupo == null) return NotFound();

            var vm = new CriarOficinaViewModel
            {
                GrupoId = grupo.Id,
                GrupoNome = grupo.Nome,
                AdminId = grupo.AdministradorId
            };

            await PopularAdminsAsync(vm.AdminId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarOficina(CriarOficinaViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var grupo = await _context.Grupos
                .Include(g => g.Oficinas)
                .FirstOrDefaultAsync(g => g.Id == model.GrupoId && g.DiretorId == user.Id);
            if (grupo == null) return NotFound();

            var limite = ObterLimitePlano(grupo.Plano);
            if (limite > 0 && grupo.Oficinas.Count >= limite)
            {
                ModelState.AddModelError(string.Empty, $"Plano {grupo.Plano} permite no máximo {limite} oficinas.");
            }

            if (!ModelState.IsValid)
            {
                model.GrupoNome = grupo.Nome;
                await PopularAdminsAsync(model.AdminId);
                return View(model);
            }

            var oficina = new Oficina
            {
                Nome = model.Nome,
                Descricao = model.Descricao,
                GrupoOficinaId = grupo.Id,
                Plano = grupo.Plano,
                AdminProprietarioId = model.AdminId
            };

            _context.Oficinas.Add(oficina);
            await _context.SaveChangesAsync();

            TempData["Msg"] = "Oficina criada com sucesso.";
            return RedirectToAction(nameof(DetalhesGrupo), new { id = grupo.Id });
        }

        public async Task<IActionResult> GerenciarUsuarios(int oficinaId)
        {
            var oficina = await ObterOficinaDoDiretorAsync(oficinaId);
            if (oficina == null) return NotFound();

            var vinculos = await _context.OficinasUsuarios
                .Where(ou => ou.OficinaId == oficina.Id)
                .Include(ou => ou.Usuario)
                .ToListAsync();

            var viewModel = new GerenciarUsuariosOficinaViewModel
            {
                OficinaId = oficina.Id,
                GrupoId = oficina.GrupoOficinaId,
                OficinaNome = oficina.Nome,
                GrupoNome = oficina.Grupo.Nome,
                Usuarios = vinculos.Select(ou => new UsuarioOficinaInfo
                {
                    VínculoId = ou.Id,
                    UsuarioId = ou.UsuarioId,
                    Nome = ObterNomeExibicao(ou.Usuario),
                    Perfil = ou.Perfil,
                    Ativo = ou.Ativo
                }).ToList()
            };

            viewModel.Disponiveis = (await ListarUsuariosDisponiveisAsync(oficina, vinculos)).ToList();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarUsuario(GerenciarUsuariosOficinaViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.UsuarioSelecionadoId))
            {
                TempData["Error"] = "Selecione um usuário.";
                return RedirectToAction(nameof(GerenciarUsuarios), new { oficinaId = model.OficinaId });
            }

            var oficina = await ObterOficinaDoDiretorAsync(model.OficinaId);
            if (oficina == null) return NotFound();

            var jaExiste = await _context.OficinasUsuarios
                .AnyAsync(ou => ou.OficinaId == oficina.Id && ou.UsuarioId == model.UsuarioSelecionadoId);
            if (jaExiste)
            {
                TempData["Error"] = "Usuário já está vinculado a esta oficina.";
                return RedirectToAction(nameof(GerenciarUsuarios), new { oficinaId = oficina.Id });
            }

            _context.OficinasUsuarios.Add(new OficinaUsuario
            {
                OficinaId = oficina.Id,
                UsuarioId = model.UsuarioSelecionadoId,
                Perfil = model.PerfilNovoUsuario,
                Ativo = true
            });
            await _context.SaveChangesAsync();

            TempData["Msg"] = "Usuário adicionado.";
            return RedirectToAction(nameof(GerenciarUsuarios), new { oficinaId = oficina.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarUsuario(int id, bool ativar)
        {
            var vinculo = await _context.OficinasUsuarios
                .Include(ou => ou.Oficina)
                .ThenInclude(o => o.Grupo)
                .FirstOrDefaultAsync(ou => ou.Id == id);

            if (vinculo == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null || vinculo.Oficina.Grupo.DiretorId != user.Id)
            {
                return Forbid();
            }

            vinculo.Ativo = ativar;
            await _context.SaveChangesAsync();
            TempData["Msg"] = ativar ? "Usuário reativado." : "Usuário desativado.";
            return RedirectToAction(nameof(GerenciarUsuarios), new { oficinaId = vinculo.OficinaId });
        }

        private int ObterLimitePlano(PlanoConta plano)
        {
            return plano switch
            {
                PlanoConta.Basico => 1,
                PlanoConta.Pro => 2,
                PlanoConta.Plus => 0,
                _ => 0
            };
        }

        private async Task<UsuarioDisponivelInfo[]> ListarUsuariosDisponiveisAsync(Oficina oficina, System.Collections.Generic.IEnumerable<OficinaUsuario> vinculados)
        {
            var vinculadosIds = vinculados.Select(v => v.UsuarioId).ToHashSet();

            var usuarios = await (from user in _context.Users
                                  join ur in _context.UserRoles on user.Id equals ur.UserId
                                  join role in _context.Roles on ur.RoleId equals role.Id
                                  where role.Name == "Admin" || role.Name == "Supervisor" || role.Name == "Mecanico"
                                  orderby user.NomeCompleto
                                  select new UsuarioDisponivelInfo
                                  {
                                      UsuarioId = user.Id,
                                      Nome = ObterNomeExibicao(user),
                                      Perfil = role.Name ?? string.Empty
                                  }).ToArrayAsync();

            return usuarios.Where(u => !vinculadosIds.Contains(u.UsuarioId)).ToArray();
        }

        private async Task PopularAdminsAsync(string? selecionado = null)
        {
            var admins = await (from user in _context.Users
                                join ur in _context.UserRoles on user.Id equals ur.UserId
                                join role in _context.Roles on ur.RoleId equals role.Id
                                where role.Name == "Admin"
                                orderby user.NomeCompleto
                                select new
                                {
                                    user.Id,
                                    Nome = ObterNomeExibicao(user)
                                }).ToListAsync();

            ViewBag.Admins = new SelectList(admins, "Id", "Nome", selecionado);
        }

        private async Task<Oficina?> ObterOficinaDoDiretorAsync(int oficinaId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            return await _context.Oficinas
                .Include(o => o.Grupo)
                .FirstOrDefaultAsync(o => o.Id == oficinaId && o.Grupo.DiretorId == user.Id);
        }

        private static string ObterNomeExibicao(ApplicationUser usuario)
        {
            if (usuario == null) return string.Empty;
            if (string.IsNullOrWhiteSpace(usuario.NomeCompleto))
            {
                return usuario.Email ?? usuario.UserName ?? usuario.Id ?? string.Empty;
            }

            return usuario.NomeCompleto;
        }
    }
}
