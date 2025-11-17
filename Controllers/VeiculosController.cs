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
using Services;

namespace projetos.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class VeiculosController : Controller
    {
        private readonly OficinaDbContext _context;
        private readonly IOficinaContext _oficinaContext;

        public VeiculosController(OficinaDbContext context, IOficinaContext oficinaContext)
        {
            _context = context;
            _oficinaContext = oficinaContext;
        }

        public async Task<IActionResult> Index()
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var veiculos = await _context.OficinasVeiculos
                .Where(ov => ov.OficinaId == oficinaId)
                .Select(ov => ov.Veiculo)
                .Include(v => v.Cliente)
                .Include(v => v.Oficinas)
                    .ThenInclude(o => o.Oficina)
                .AsNoTracking()
                .ToListAsync();
            return View(veiculos);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var oficinaId = await ObterOficinaAtualIdAsync();
            var veiculo = await _context.Veiculos
                .Include(v => v.Cliente)
                .Include(v => v.Oficinas)
                .ThenInclude(o => o.Oficina)
                .FirstOrDefaultAsync(v => v.Id == id && v.Oficinas.Any(o => o.OficinaId == oficinaId));

            if (veiculo == null) return NotFound();
            return View(veiculo);
        }

        public async Task<IActionResult> Create()
        {
            await PopularClientesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Placa,Marca,Modelo,Ano,ClienteId")] Veiculo veiculo)
        {
            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Cliente")).ToList())
            {
                ModelState.Remove(key);
            }

            if (ModelState.IsValid)
            {
                var oficinaId = await ObterOficinaAtualIdAsync();
                _context.Veiculos.Add(veiculo);
                await _context.SaveChangesAsync();

                _context.OficinasVeiculos.Add(new OficinaVeiculo
                {
                    OficinaId = oficinaId,
                    VeiculoId = veiculo.Id,
                    VinculadoEm = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["Success"] = "Veículo cadastrado e vinculado à oficina.";
                TempData["ClearTableState"] = true;
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Verifique os dados informados.";
            await PopularClientesAsync(veiculo.ClienteId);
            return View(veiculo);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var oficinaId = await ObterOficinaAtualIdAsync();
            var veiculo = await _context.Veiculos
                .FirstOrDefaultAsync(v => v.Id == id && v.Oficinas.Any(o => o.OficinaId == oficinaId));
            if (veiculo == null) return NotFound();

            await PopularClientesAsync(veiculo.ClienteId);
            return View(veiculo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Placa,Marca,Modelo,Ano,ClienteId")] Veiculo veiculo)
        {
            if (id != veiculo.Id) return NotFound();

            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Cliente")).ToList())
            {
                ModelState.Remove(key);
            }

            if (ModelState.IsValid)
            {
                var oficinaId = await ObterOficinaAtualIdAsync();
                var original = await _context.Veiculos
                    .FirstOrDefaultAsync(v => v.Id == id && v.Oficinas.Any(o => o.OficinaId == oficinaId));
                if (original == null) return NotFound();

                _context.Entry(original).CurrentValues.SetValues(veiculo);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Veículo atualizado.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Dados inválidos na edição.";
            await PopularClientesAsync(veiculo.ClienteId);
            return View(veiculo);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var oficinaId = await ObterOficinaAtualIdAsync();
            var veiculo = await _context.Veiculos
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.Id == id && v.Oficinas.Any(o => o.OficinaId == oficinaId));

            if (veiculo == null) return NotFound();
            return View(veiculo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var veiculo = await _context.Veiculos
                .Include(v => v.Oficinas)
                .FirstOrDefaultAsync(v => v.Id == id && v.Oficinas.Any(o => o.OficinaId == oficinaId));

            if (veiculo == null)
            {
                TempData["Error"] = "Veículo não localizado.";
                return RedirectToAction(nameof(Index));
            }

            _context.Veiculos.Remove(veiculo);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Veículo removido.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Vincular(string? busca)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var query = _context.Veiculos
                .Where(v => !v.Oficinas.Any(o => o.OficinaId == oficinaId));

            if (!string.IsNullOrWhiteSpace(busca))
            {
                query = query.Where(v =>
                    v.Placa.Contains(busca) ||
                    v.Modelo.Contains(busca) ||
                    v.Marca.Contains(busca));
            }

            var candidatos = await query
                .Select(v => new
                {
                    v.Id,
                    v.Placa,
                    v.Modelo,
                    Cliente = v.Cliente != null ? v.Cliente.Nome : string.Empty,
                    Origem = v.Oficinas.Select(o => o.Oficina.Nome)
                })
                .OrderBy(v => v.Placa)
                .ToListAsync();

            var viewModel = new VincularVeiculoViewModel
            {
                Busca = busca,
                Veiculos = candidatos.Select(v => new VeiculoDisponivelViewModel
                {
                    Id = v.Id,
                    Placa = v.Placa,
                    Modelo = v.Modelo,
                    Cliente = v.Cliente,
                    Origem = v.Origem.Any() ? string.Join(", ", v.Origem) : "Sem origem definida"
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VincularVeiculo(int veiculoId)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var existe = await _context.OficinasVeiculos
                .AnyAsync(ov => ov.OficinaId == oficinaId && ov.VeiculoId == veiculoId);
            if (existe)
            {
                TempData["Error"] = "Veículo já disponível nesta oficina.";
                return RedirectToAction(nameof(Index));
            }

            var veiculo = await _context.Veiculos.FindAsync(veiculoId);
            if (veiculo == null)
            {
                TempData["Error"] = "Veículo não encontrado.";
                return RedirectToAction(nameof(Vincular));
            }

            _context.OficinasVeiculos.Add(new OficinaVeiculo
            {
                OficinaId = oficinaId,
                VeiculoId = veiculoId,
                VinculadoEm = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Veículo vinculado.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<int> ObterOficinaAtualIdAsync()
        {
            var oficina = await _oficinaContext.GetOficinaAtualAsync();
            if (oficina == null)
            {
                throw new InvalidOperationException("Nenhuma oficina selecionada.");
            }
            return oficina.Id;
        }

        private async Task PopularClientesAsync(int? selecionado = null)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var clientes = await _context.OficinasClientes
                .Where(oc => oc.OficinaId == oficinaId)
                .Select(oc => new
                {
                    oc.Cliente.Id,
                    oc.Cliente.Nome
                })
                .OrderBy(c => c.Nome)
                .ToListAsync();

            ViewData["ClienteId"] = new SelectList(clientes, "Id", "Nome", selecionado);
        }
    }
}
