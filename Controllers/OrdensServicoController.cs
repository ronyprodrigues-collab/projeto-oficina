using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data;
using Models;
using Microsoft.AspNetCore.Identity;
using Models.ViewModels;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.Security.Claims;

namespace projetos.Controllers
{
    public class OrdensServicoController : Controller
    {
        private readonly OficinaDbContext _context;
        private readonly IConverter? _pdfConverter;

        public OrdensServicoController(OficinaDbContext context, IConverter? pdfConverter = null)
        {
            _context = context;
            _pdfConverter = pdfConverter;
        }

        // GET: OrdensServico
        public async Task<IActionResult> Index(bool? mine, string? status, int? clienteId, int? veiculoId)
        {
            var query = _context.OrdensServico.AsQueryable();
            if (mine == true && User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Where(o => o.MecanicoId == userId);
                }
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status == status);
            }
            if (clienteId.HasValue)
            {
                query = query.Where(o => o.ClienteId == clienteId.Value);
            }
            if (veiculoId.HasValue)
            {
                query = query.Where(o => o.VeiculoId == veiculoId.Value);
            }
            var oficinaDbContext = query
                .Include(o => o.Cliente)
                .Include(o => o.Mecanico)
                .Include(o => o.Veiculo);
            return View(await oficinaDbContext.ToListAsync());
        }

        // GET: OrdensServico/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordemServico = await _context.OrdensServico
                .Include(o => o.Cliente)
                .Include(o => o.Mecanico)
                .Include(o => o.Veiculo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ordemServico == null)
            {
                return NotFound();
            }

            return View(ordemServico);
        }

        // GET: OrdensServico/Create
        public IActionResult Create()
        {
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Id");
            ViewData["MecanicoId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["VeiculoId"] = new SelectList(_context.Veiculos, "Id", "Id");
            return View();
        }

        // POST: OrdensServico/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClienteId,VeiculoId,MecanicoId,Descricao,DataAbertura,DataPrevista,DataConclusao,Status,AprovadaCliente,Observacoes")] OrdemServico ordemServico, List<ServicoItemInput>? servicos, List<PecaItemInput>? pecas)
        {
            if (ModelState.IsValid)
            {
                ordemServico.DataAbertura = DateTime.UtcNow;
                _context.Add(ordemServico);
                await _context.SaveChangesAsync();
                if (servicos != null)
                {
                    foreach (var s in servicos.Where(s => !string.IsNullOrWhiteSpace(s.Descricao)))
                    {
                        _context.ServicoItem.Add(new ServicoItem { Descricao = s.Descricao, Valor = s.Valor, OrdemServicoId = ordemServico.Id });
                    }
                }
                if (pecas != null)
                {
                    foreach (var p in pecas.Where(p => !string.IsNullOrWhiteSpace(p.Nome)))
                    {
                        _context.PecaItem.Add(new PecaItem { Nome = p.Nome, ValorUnitario = p.ValorUnitario, Quantidade = p.Quantidade, OrdemServicoId = ordemServico.Id });
                    }
                }
                if ((servicos?.Count ?? 0) > 0 || (pecas?.Count ?? 0) > 0)
                {
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Id", ordemServico.ClienteId);
            ViewData["MecanicoId"] = new SelectList(_context.Users, "Id", "Id", ordemServico.MecanicoId);
            ViewData["VeiculoId"] = new SelectList(_context.Veiculos, "Id", "Id", ordemServico.VeiculoId);
            return View(ordemServico);
        }

        // GET: OrdensServico/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordemServico = await _context.OrdensServico.FindAsync(id);
            if (ordemServico == null)
            {
                return NotFound();
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Id", ordemServico.ClienteId);
            ViewData["MecanicoId"] = new SelectList(_context.Users, "Id", "Id", ordemServico.MecanicoId);
            ViewData["VeiculoId"] = new SelectList(_context.Veiculos, "Id", "Id", ordemServico.VeiculoId);
            return View(ordemServico);
        }

        // POST: OrdensServico/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,VeiculoId,MecanicoId,Descricao,DataAbertura,DataPrevista,DataConclusao,Status,AprovadaCliente,Observacoes")] OrdemServico ordemServico)
        {
            if (id != ordemServico.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ordemServico);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdemServicoExists(ordemServico.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Id", ordemServico.ClienteId);
            ViewData["MecanicoId"] = new SelectList(_context.Users, "Id", "Id", ordemServico.MecanicoId);
            ViewData["VeiculoId"] = new SelectList(_context.Veiculos, "Id", "Id", ordemServico.VeiculoId);
            return View(ordemServico);
        }

        // GET: OrdensServico/AssignMechanic/5
        public async Task<IActionResult> AssignMechanic(int? id)
        {
            if (id == null) return NotFound();
            var ordem = await _context.OrdensServico.FindAsync(id);
            if (ordem == null) return NotFound();

            var roleId = await _context.Roles.Where(r => r.Name == "Mecanico").Select(r => r.Id).FirstOrDefaultAsync();
            var mecanicos = await (from u in _context.Users
                                   join ur in _context.UserRoles on u.Id equals ur.UserId
                                   where ur.RoleId == roleId
                                   select u).ToListAsync();
            ViewData["MecanicoId"] = new SelectList(mecanicos, "Id", "Email", ordem.MecanicoId);
            return View(ordem);
        }

        // POST: OrdensServico/AssignMechanic/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMechanic(int id, string mecanicoId)
        {
            var ordem = await _context.OrdensServico.FindAsync(id);
            if (ordem == null) return NotFound();
            ordem.MecanicoId = mecanicoId;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: OrdensServico/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var ordem = await _context.OrdensServico.FindAsync(id);
            if (ordem == null) return NotFound();
            ordem.AprovadaCliente = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: OrdensServico/Conclude/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Conclude(int id)
        {
            var ordem = await _context.OrdensServico.FindAsync(id);
            if (ordem == null) return NotFound();
            ordem.DataConclusao = DateTime.UtcNow;
            ordem.Status = string.IsNullOrEmpty(ordem.Status) ? "Concluida" : ordem.Status;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: OrdensServico/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordemServico = await _context.OrdensServico
                .Include(o => o.Cliente)
                .Include(o => o.Mecanico)
                .Include(o => o.Veiculo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ordemServico == null)
            {
                return NotFound();
            }

            return View(ordemServico);
        }

        // POST: OrdensServico/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ordemServico = await _context.OrdensServico.FindAsync(id);
            if (ordemServico != null)
            {
                _context.OrdensServico.Remove(ordemServico);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrdemServicoExists(int id)
        {
            return _context.OrdensServico.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprovarCliente(int id)
        {
            var ordem = await _context.OrdensServico.FindAsync(id);
            if (ordem == null) return NotFound();
            ordem.AprovadaCliente = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtribuirMecanico(int id, string mecanicoId)
        {
            var ordem = await _context.OrdensServico.FindAsync(id);
            if (ordem == null) return NotFound();
            ordem.MecanicoId = mecanicoId;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Concluir(int id)
        {
            var ordem = await _context.OrdensServico.FindAsync(id);
            if (ordem == null) return NotFound();
            ordem.DataConclusao = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(ordem.Status)) ordem.Status = "Concluida";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> GerarPDF(int id)
        {
            var os = await _context.OrdensServico
                .Include(o => o.Cliente)
                .Include(o => o.Veiculo)
                .Include(o => o.Mecanico)
                .Include(o => o.Servicos)
                .Include(o => o.Pecas)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (os == null) return NotFound();

            var totalServicos = os.Servicos?.Sum(s => s.Valor) ?? 0m;
            var totalPecas = os.Pecas?.Sum(p => p.ValorUnitario * p.Quantidade) ?? 0m;
            var total = totalServicos + totalPecas;

            var html = $@"<!DOCTYPE html><html><head><meta charset='utf-8'><style>
                body{{font-family:Arial, sans-serif;}} h1{{font-size:20px;}}
                table{{width:100%;border-collapse:collapse;margin-top:10px}}
                th,td{{border:1px solid #ccc;padding:6px;text-align:left}}
                .right{{text-align:right}}
                </style></head><body>
                <h1>Ordem de Serviço #{os.Id}</h1>
                <p><strong>Cliente:</strong> {os.Cliente?.Nome} | <strong>Veículo:</strong> {os.Veiculo?.Marca} {os.Veiculo?.Modelo} ({os.Veiculo?.Placa})</p>
                <p><strong>Mecânico:</strong> {os.Mecanico?.NomeCompleto ?? os.Mecanico?.Email} | <strong>Status:</strong> {os.Status}</p>
                <p><strong>Abertura:</strong> {os.DataAbertura} | <strong>Conclusão:</strong> {os.DataConclusao}</p>
                <h3>Serviços</h3>
                <table><thead><tr><th>Descrição</th><th class='right'>Valor</th></tr></thead><tbody>
                {string.Join("", (os.Servicos??new()).Select(s => $"<tr><td>{s.Descricao}</td><td class='right'>{s.Valor:C}</td></tr>"))}
                <tr><td><strong>Total</strong></td><td class='right'><strong>{totalServicos:C}</strong></td></tr>
                </tbody></table>
                <h3>Peças</h3>
                <table><thead><tr><th>Nome</th><th>Qtd</th><th class='right'>Vlr.Un</th><th class='right'>Total</th></tr></thead><tbody>
                {string.Join("", (os.Pecas??new()).Select(p => $"<tr><td>{p.Nome}</td><td>{p.Quantidade}</td><td class='right'>{p.ValorUnitario:C}</td><td class='right'>{(p.Quantidade*p.ValorUnitario):C}</td></tr>"))}
                <tr><td colspan='3'><strong>Total</strong></td><td class='right'><strong>{totalPecas:C}</strong></td></tr>
                </tbody></table>
                <h2 class='right'>Total Geral: {total:C}</h2>
                </body></html>";

            if (_pdfConverter == null)
            {
                return Content("Conversor de PDF não configurado. Instale e configure DinkToPdf.", "text/plain");
            }

            var doc = new HtmlToPdfDocument
            {
                GlobalSettings = new GlobalSettings { ColorMode = ColorMode.Color, Orientation = Orientation.Portrait, PaperSize = PaperKind.A4 },
                Objects = { new ObjectSettings { HtmlContent = html } }
            };
            var bytes = _pdfConverter.Convert(doc);
            return File(bytes, "application/pdf", $"OS_{os.Id}.pdf");
        }

        // GET: OrdensServico/Minhas
        public async Task<IActionResult> Minhas()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var lista = await _context.OrdensServico
                .Include(o => o.Veiculo)
                .Include(o => o.Servicos)
                .Where(o => o.MecanicoId == userId)
                .OrderBy(o => o.DataPrevista)
                .ToListAsync();

            return View(lista);
        }
    }
}


