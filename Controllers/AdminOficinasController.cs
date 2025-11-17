using System.Linq;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;

namespace projetos.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOficinasController : Controller
    {
        private readonly OficinaDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminOficinasController(OficinaDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var oficinas = await _context.Oficinas
                .Include(o => o.Grupo)
                .Where(o =>
                    o.AdminProprietarioId == user.Id ||
                    o.Usuarios.Any(ou => ou.UsuarioId == user.Id))
                .OrderBy(o => o.Nome)
                .ToListAsync();

            return View(oficinas);
        }

        public async Task<IActionResult> GerenciarUsuarios(int oficinaId)
        {
            var oficina = await ObterOficinaPermitidaAsync(oficinaId);
            if (oficina == null) return NotFound();

            var vinculos = await _context.OficinasUsuarios
                .Where(ou => ou.OficinaId == oficinaId)
                .Include(ou => ou.Usuario)
                .ToListAsync();

            var viewModel = new GerenciarUsuariosOficinaViewModel
            {
                OficinaId = oficina.Id,
                GrupoId = oficina.GrupoOficinaId,
                OficinaNome = oficina.Nome,
                GrupoNome = oficina.Grupo?.Nome ?? string.Empty,
                Usuarios = vinculos.Select(ou => new UsuarioOficinaInfo
                {
                    VínculoId = ou.Id,
                    UsuarioId = ou.UsuarioId,
                    Nome = ObterNomeExibicao(ou.Usuario),
                    Perfil = ou.Perfil,
                    Ativo = ou.Ativo
                }).ToList()
            };

            viewModel.Disponiveis = (await ListarUsuariosDisponiveisAsync(vinculos)).ToList();
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

            var oficina = await ObterOficinaPermitidaAsync(model.OficinaId);
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

            var oficina = await ObterOficinaPermitidaAsync(vinculo.OficinaId);
            if (oficina == null) return Forbid();

            vinculo.Ativo = ativar;
            await _context.SaveChangesAsync();

            TempData["Msg"] = ativar ? "Usuário reativado." : "Usuário desativado.";
            return RedirectToAction(nameof(GerenciarUsuarios), new { oficinaId = vinculo.OficinaId });
        }

        private async Task<Oficina?> ObterOficinaPermitidaAsync(int oficinaId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            return await _context.Oficinas
                .Include(o => o.Grupo)
                .FirstOrDefaultAsync(o =>
                    o.Id == oficinaId &&
                    (o.AdminProprietarioId == user.Id ||
                     o.Usuarios.Any(ou => ou.UsuarioId == user.Id)));
        }

        private async Task<UsuarioDisponivelInfo[]> ListarUsuariosDisponiveisAsync(System.Collections.Generic.IEnumerable<OficinaUsuario> vinculados)
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
