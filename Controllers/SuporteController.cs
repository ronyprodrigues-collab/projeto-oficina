using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

namespace projetos.Controllers
{
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
        public async Task<IActionResult> Index()
        {
            var cfg = await _context.Configuracoes.FirstOrDefaultAsync() ?? new Configuracoes();
            return View(cfg);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarLogo(IFormFile? logo)
        {
            if (logo == null || logo.Length == 0)
                return RedirectToAction(nameof(Index));

            var uploadsDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"logo_{System.DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(logo.FileName)}";
            var fullPath = Path.Combine(uploadsDir, fileName);
            using (var stream = System.IO.File.Create(fullPath))
            {
                await logo.CopyToAsync(stream);
            }
            var relativePath = $"/uploads/{fileName}";

            var cfg = await _context.Configuracoes.FirstOrDefaultAsync();
            if (cfg == null)
            {
                cfg = new Configuracoes { LogoPath = relativePath };
                _context.Configuracoes.Add(cfg);
            }
            else
            {
                cfg.LogoPath = relativePath;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarCores(string? corPrimaria, string? corSecundaria, string? nomeOficina)
        {
            var cfg = await _context.Configuracoes.FirstOrDefaultAsync();
            if (cfg == null)
            {
                cfg = new Configuracoes();
                _context.Configuracoes.Add(cfg);
            }
            if (!string.IsNullOrWhiteSpace(corPrimaria)) cfg.CorPrimaria = corPrimaria!;
            if (!string.IsNullOrWhiteSpace(corSecundaria)) cfg.CorSecundaria = corSecundaria!;
            if (!string.IsNullOrWhiteSpace(nomeOficina)) cfg.NomeOficina = nomeOficina!;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
