using System.Linq;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;

namespace projetos.Controllers
{
    [Authorize(Roles = "SuporteTecnico")]
    public class GruposController : Controller
    {
        private readonly OficinaDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GruposController(OficinaDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var grupos = await _context.Grupos
                .Include(g => g.Diretor)
                .Include(g => g.Oficinas)
                .AsNoTracking()
                .OrderBy(g => g.Nome)
                .ToListAsync();
            return View(grupos);
        }

        public async Task<IActionResult> Create()
        {
            await PopularDiretoresAsync();
            return View(new GrupoOficina
            {
                Plano = PlanoConta.Basico,
                CorPrimaria = "#0d6efd",
                CorSecundaria = "#6c757d"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GrupoOficina grupo)
        {
            if (ModelState.IsValid)
            {
                _context.Grupos.Add(grupo);
                await _context.SaveChangesAsync();
                TempData["Msg"] = "Grupo criado com sucesso.";
                return RedirectToAction(nameof(Index));
            }

            await PopularDiretoresAsync(grupo.DiretorId);
            return View(grupo);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var grupo = await _context.Grupos.FindAsync(id);
            if (grupo == null) return NotFound();

            await PopularDiretoresAsync(grupo.DiretorId);
            return View(grupo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GrupoOficina grupo)
        {
            if (id != grupo.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grupo);
                    await _context.SaveChangesAsync();
                    TempData["Msg"] = "Grupo atualizado.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Grupos.AnyAsync(g => g.Id == id))
                        return NotFound();
                    throw;
                }
            }

            await PopularDiretoresAsync(grupo.DiretorId);
            return View(grupo);
        }

        private async Task PopularDiretoresAsync(string? selecionado = null)
        {
            var diretores = await (from user in _context.Users
                                   join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                   join role in _context.Roles on userRole.RoleId equals role.Id
                                   where role.Name == "Diretor"
                                   orderby user.NomeCompleto
                                   select new
                                   {
                                       user.Id,
                                       Nome = string.IsNullOrWhiteSpace(user.NomeCompleto) ? user.Email : user.NomeCompleto
                                   }).ToListAsync();

            ViewBag.Diretores = new SelectList(diretores, "Id", "Nome", selecionado);
        }
    }
}
