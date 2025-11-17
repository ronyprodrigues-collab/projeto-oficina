using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public SuporteController(OficinaDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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

            var oficinas = grupos.First(g => g.Id == grupoSelecionadoId)
                .Oficinas
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
                Defaults = defaults
            };
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
        public async Task<IActionResult> AlterarCores(int grupoId, int oficinaId, string? corPrimaria, string? corSecundaria, string? nomeOficina)
        {
            var oficina = await _context.Oficinas.FirstOrDefaultAsync(o => o.Id == oficinaId);
            if (oficina == null)
                return RedirectToAction(nameof(Index), new { grupoId, oficinaId });

            if (!string.IsNullOrWhiteSpace(corPrimaria)) oficina.CorPrimaria = corPrimaria!;
            if (!string.IsNullOrWhiteSpace(corSecundaria)) oficina.CorSecundaria = corSecundaria!;
            if (!string.IsNullOrWhiteSpace(nomeOficina)) oficina.Nome = nomeOficina!;
            await _context.SaveChangesAsync();
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
    }
}
