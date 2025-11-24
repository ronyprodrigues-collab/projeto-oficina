using System;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;

namespace projetos.Controllers
{
    [Authorize(Roles = "Admin,SuporteTecnico,Diretor")]
    public class ConvitesController : Controller
    {
        private readonly OficinaDbContext _context;

        public ConvitesController(OficinaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var convites = await _context.Convites
                .OrderByDescending(c => c.CriadoEm)
                .Take(100)
                .ToListAsync();
            return View(convites);
        }

        public IActionResult Create()
        {
            var vm = new ConviteInputViewModel();
            PopularPerfis(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConviteInputViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopularPerfis(model);
                return View(model);
            }

            var convite = new Convite
            {
                Email = model.Email.Trim(),
                PerfilDestino = model.PerfilDestino,
                PercentualComissao = model.PercentualComissao,
                ExpiraEm = model.ExpiraEm
            };
            _context.Convites.Add(convite);
            await _context.SaveChangesAsync();
            TempData["Msg"] = "Convite criado. Compartilhe o token com o convidado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var convite = await _context.Convites.FindAsync(id);
            if (convite == null) return NotFound();
            if (convite.Usado)
            {
                TempData["Error"] = "Convite já foi utilizado.";
            }
            else
            {
                _context.Convites.Remove(convite);
                await _context.SaveChangesAsync();
                TempData["Msg"] = "Convite cancelado.";
            }
            return RedirectToAction(nameof(Index));
        }

        private void PopularPerfis(ConviteInputViewModel model)
        {
            var perfis = new[]
            {
                new SelectListItem("Administrador", "Admin"),
                new SelectListItem("Diretor", "Diretor"),
                new SelectListItem("Supervisor", "Supervisor")
            };
            model.Perfis = new SelectList(perfis, "Value", "Text", model.PerfilDestino);
        }
    }
}
