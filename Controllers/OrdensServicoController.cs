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
using Microsoft.AspNetCore.Authorization;
using Models.ViewModels;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.Security.Claims;
using Services;

namespace projetos.Controllers
{
    [Authorize]
    public class OrdensServicoController : Controller
    {
        private readonly OficinaDbContext _context;
        private readonly IConverter? _pdfConverter;
        private readonly IEstoqueService _estoqueService;
        private readonly IOficinaContext _oficinaContext;

        public OrdensServicoController(
            OficinaDbContext context,
            IConverter? pdfConverter = null,
            IEstoqueService? estoqueService = null,
            IOficinaContext? oficinaContext = null)
        {
            _context = context;
            _pdfConverter = pdfConverter;
            _estoqueService = estoqueService ?? throw new ArgumentNullException(nameof(estoqueService));
            _oficinaContext = oficinaContext ?? throw new ArgumentNullException(nameof(oficinaContext));
        }

        [Authorize]
        public async Task<IActionResult> Index(bool? mine, string? status, int? clienteId, int? veiculoId, bool? pendentes)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var query = _context.OrdensServico
                .Where(o => o.OficinaId == oficinaId);
            var isMecanico = User.IsInRole("Mecanico");
            var isAdmin = User.IsInRole("Admin");
            var isSupervisor = User.IsInRole("Supervisor");

            if (isMecanico)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();
                query = query.Where(o => o.MecanicoId == userId && o.AprovadaCliente);
            }
            else if (mine == true && User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Where(o => o.MecanicoId == userId);
                }
            }

            if (pendentes == true)
            {
                query = query.Where(o => !o.AprovadaCliente && o.DataConclusao == null);
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

            if (isAdmin || isSupervisor)
            {
                ViewBag.Pendentes = await _context.OrdensServico
                    .Include(o => o.Cliente)
                    .Include(o => o.Veiculo)
                    .Where(o => o.OficinaId == oficinaId && !o.AprovadaCliente && string.IsNullOrWhiteSpace(o.MotivoReprovacao))
                    .OrderByDescending(o => o.DataAbertura)
                    .ToListAsync();
            }

            var oficinaDbContext = query
                .Include(o => o.Cliente)
                .Include(o => o.Mecanico)
                .Include(o => o.Veiculo);
            return View(await oficinaDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var oficinaId = await ObterOficinaAtualIdAsync();
            var ordemServico = await _context.OrdensServico
                .Include(o => o.Cliente)
                .Include(o => o.Mecanico)
                .Include(o => o.Veiculo)
                .Include(o => o.Servicos)
                .Include(o => o.Pecas)
                .FirstOrDefaultAsync(m => m.Id == id && m.OficinaId == oficinaId);
            if (ordemServico == null)
            {
                return NotFound();
            }

            return View(ordemServico);
        }

        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> Create()
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            await PopularDadosFormularioAsync(oficinaId);
            var model = new OrdemServico
            {
                DataPrimeiroVencimento = DateTime.Today
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> Create([Bind("Id,ClienteId,VeiculoId,MecanicoId,Descricao,DataAbertura,DataPrevista,DataConclusao,Status,AprovadaCliente,Observacoes,FormaPagamento,QuantidadeParcelas,DataPrimeiroVencimento,ContaRecebimentoId")] OrdemServico ordemServico, List<ServicoItemInput>? servicos, List<PecaItemInput>? pecas)
        {
            ordemServico.DataPrimeiroVencimento ??= DateTime.Today;
            if (ordemServico.QuantidadeParcelas < 1)
            {
                ModelState.AddModelError(nameof(ordemServico.QuantidadeParcelas), "Informe a quantidade de parcelas.");
            }

            if (ModelState.IsValid)
            {
                var oficinaId = await ObterOficinaAtualIdAsync();
                ordemServico.DataAbertura = DateTime.UtcNow;
                if (!ordemServico.AprovadaCliente && string.IsNullOrWhiteSpace(ordemServico.Status))
                {
                    ordemServico.Status = "Pendente de aprovação";
                }
                ordemServico.OficinaId = oficinaId;
                _context.Add(ordemServico);
                await _context.SaveChangesAsync();
                if (servicos != null)
                {
                    foreach (var s in servicos.Where(s => !string.IsNullOrWhiteSpace(s.Descricao)))
                    {
                        _context.ServicoItem.Add(new ServicoItem { Descricao = s.Descricao, Valor = s.Valor, OrdemServicoId = ordemServico.Id });
                    }
                }
                var pecasProcessadas = await MontarPecaItensAsync(pecas);
                foreach (var registro in pecasProcessadas)
                {
                    registro.Item.OrdemServicoId = ordemServico.Id;
                    _context.PecaItem.Add(registro.Item);
                }
                if ((servicos?.Count ?? 0) > 0 || pecasProcessadas.Count > 0)
                {
                    await _context.SaveChangesAsync();
                    await BaixarEstoqueAsync(pecasProcessadas, ordemServico.Id);
                    ordemServico.EstoqueReservado = pecasProcessadas.Any(p => p.PecaEstoqueId.HasValue);
                    _context.Update(ordemServico);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            var oficinaAtual = await ObterOficinaAtualIdAsync();
            await PopularDadosFormularioAsync(oficinaAtual, ordemServico.ClienteId, ordemServico.MecanicoId, ordemServico.VeiculoId, ordemServico.ContaRecebimentoId);
            return View(ordemServico);
        }

        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var oficinaId = await ObterOficinaAtualIdAsync();
            var ordemServico = await _context.OrdensServico
                .Include(o => o.Servicos)
                .Include(o => o.Pecas)
                .FirstOrDefaultAsync(o => o.Id == id && o.OficinaId == oficinaId);
            if (ordemServico == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrWhiteSpace(ordemServico.MotivoReprovacao))
            {
                TempData["Error"] = "Ordem reprovada não pode ser editada.";
                return RedirectToAction(nameof(Details), new { id });
            }
            await PopularDadosFormularioAsync(oficinaId, ordemServico.ClienteId, ordemServico.MecanicoId, ordemServico.VeiculoId, ordemServico.ContaRecebimentoId);
            return View(ordemServico);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,VeiculoId,MecanicoId,Descricao,DataPrevista,Status,AprovadaCliente,Observacoes,FormaPagamento,QuantidadeParcelas,DataPrimeiroVencimento,ContaRecebimentoId")] OrdemServico ordemServico, List<ServicoItemInput>? servicos, List<PecaItemInput>? pecas)
        {
            if (id != ordemServico.Id)
            {
                return NotFound();
            }

            ordemServico.DataPrimeiroVencimento ??= DateTime.Today;
            if (ordemServico.QuantidadeParcelas < 1)
            {
                ModelState.AddModelError(nameof(ordemServico.QuantidadeParcelas), "Informe a quantidade de parcelas.");
            }

            var oficinaId = await ObterOficinaAtualIdAsync();

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.OrdensServico
                        .Include(o => o.Servicos)
                        .Include(o => o.Pecas)
                        .FirstOrDefaultAsync(o => o.Id == id && o.OficinaId == oficinaId);
                    if (existing == null) return NotFound();
                    if (!string.IsNullOrWhiteSpace(existing.MotivoReprovacao))
                    {
                        TempData["Error"] = "Ordem reprovada não pode ser editada.";
                        return RedirectToAction(nameof(Details), new { id });
                    }

                    existing.ClienteId = ordemServico.ClienteId;
                    existing.VeiculoId = ordemServico.VeiculoId;
                    existing.MecanicoId = ordemServico.MecanicoId;
                    existing.Descricao = ordemServico.Descricao;
                    existing.DataPrevista = ordemServico.DataPrevista;
                    existing.Status = string.IsNullOrWhiteSpace(ordemServico.Status) ? existing.Status : ordemServico.Status;
                    existing.AprovadaCliente = ordemServico.AprovadaCliente;
                    existing.Observacoes = ordemServico.Observacoes;

                    var devolucaoAnterior = await ReporEstoqueAsync(existing.Id);
                    if (devolucaoAnterior)
                    {
                        existing.EstoqueReservado = false;
                    }

                    _context.ServicoItem.RemoveRange(existing.Servicos);
                    _context.PecaItem.RemoveRange(existing.Pecas);

                    if (servicos != null)
                    {
                        foreach (var s in servicos.Where(s => !string.IsNullOrWhiteSpace(s.Descricao)))
                        {
                            _context.ServicoItem.Add(new ServicoItem { Descricao = s.Descricao, Valor = s.Valor, OrdemServicoId = existing.Id });
                        }
                    }
                    var novasPecas = await MontarPecaItensAsync(pecas);
                    foreach (var registro in novasPecas)
                    {
                        registro.Item.OrdemServicoId = existing.Id;
                        _context.PecaItem.Add(registro.Item);
                    }

                    await _context.SaveChangesAsync();
                    await BaixarEstoqueAsync(novasPecas, existing.Id);
                    existing.EstoqueReservado = novasPecas.Any(p => p.PecaEstoqueId.HasValue);
                    _context.Update(existing);
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
            await PopularDadosFormularioAsync(oficinaId, ordemServico.ClienteId, ordemServico.MecanicoId, ordemServico.VeiculoId, ordemServico.ContaRecebimentoId);
            return View(ordemServico);
        }

        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> AssignMechanic(int? id)
        {
            if (id == null) return NotFound();
            var oficinaId = await ObterOficinaAtualIdAsync();
            var ordem = await _context.OrdensServico
                .FirstOrDefaultAsync(o => o.Id == id && o.OficinaId == oficinaId);
            if (ordem == null) return NotFound();

            var roleId = await _context.Roles.Where(r => r.Name == "Mecanico").Select(r => r.Id).FirstOrDefaultAsync();
            var mecanicos = await (from u in _context.Users
                                   join ur in _context.UserRoles on u.Id equals ur.UserId
                                   where ur.RoleId == roleId
                                   select u).ToListAsync();
            ViewData["MecanicoId"] = new SelectList(mecanicos, "Id", "NomeCompleto", ordem.MecanicoId);
            return View(ordem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> AssignMechanic(int id, string mecanicoId)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var ordem = await _context.OrdensServico
                .FirstOrDefaultAsync(o => o.Id == id && o.OficinaId == oficinaId);
            if (ordem == null) return NotFound();
            ordem.MecanicoId = mecanicoId;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> Approve(int id)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var ordem = await _context.OrdensServico
                .Include(o => o.Pecas)
                .FirstOrDefaultAsync(o => o.Id == id && o.OficinaId == oficinaId);
            if (ordem == null) return NotFound();

            if (!ordem.AprovadaCliente)
            {
                if (!ordem.EstoqueReservado)
                {
                    var registros = ConverterParaProcessadas(ordem.Pecas).ToList();
                    await BaixarEstoqueAsync(registros, ordem.Id);
                    ordem.EstoqueReservado = registros.Any();
                }
                ordem.AprovadaCliente = true;
                ordem.MotivoReprovacao = null;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> Reject(int id, string motivoReprovacao)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var ordem = await _context.OrdensServico
                .Include(o => o.Pecas)
                .FirstOrDefaultAsync(o => o.Id == id && o.OficinaId == oficinaId);
            if (ordem == null) return NotFound();

            if (string.IsNullOrWhiteSpace(motivoReprovacao))
            {
                TempData["Error"] = "Informe o motivo da reprovação.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (ordem.EstoqueReservado)
            {
                var devolvido = await ReporEstoqueAsync(ordem.Id);
                if (devolvido)
                {
                    ordem.EstoqueReservado = false;
                }
            }
            ordem.AprovadaCliente = false;
            ordem.MotivoReprovacao = motivoReprovacao;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Conclude(int id)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var ordem = await _context.OrdensServico
                .Include(o => o.Servicos)
                .Include(o => o.Pecas)
                .Include(o => o.Cliente)
                .Include(o => o.Mecanico)
                .Include(o => o.Oficina)
                .FirstOrDefaultAsync(o => o.Id == id && o.OficinaId == oficinaId);
            if (ordem == null) return NotFound();
            ordem.DataConclusao = DateTime.UtcNow;
            ordem.Status = string.IsNullOrEmpty(ordem.Status) ? "Concluida" : ordem.Status;
            await _context.SaveChangesAsync();
            await GerarLancamentosFinanceirosDaOsAsync(ordem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var oficinaId = await ObterOficinaAtualIdAsync();
            var ordemServico = await _context.OrdensServico
                .Include(o => o.Cliente)
                .Include(o => o.Mecanico)
                .Include(o => o.Veiculo)
                .FirstOrDefaultAsync(m => m.Id == id && m.OficinaId == oficinaId);
            if (ordemServico == null)
            {
                return NotFound();
            }

            return View(ordemServico);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var ordemServico = await _context.OrdensServico
                .Include(o => o.Pecas)
                .FirstOrDefaultAsync(o => o.Id == id && o.OficinaId == oficinaId);
            if (ordemServico != null)
            {
                await ReporEstoqueAsync(ordemServico.Id);
                _context.OrdensServico.Remove(ordemServico);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OrdemServicoExists(int id)
        {
            return _context.OrdensServico.Any(e => e.Id == id);
        }

        private class PecaProcessada
        {
            public PecaItem Item { get; set; } = null!;
            public int? PecaEstoqueId { get; set; }
        }

        private async Task BaixarEstoqueAsync(IEnumerable<PecaProcessada> pecas, int? ordemServicoId)
        {
            if (_estoqueService == null) return;
            foreach (var registro in pecas)
            {
                if (registro.PecaEstoqueId.HasValue)
                {
                    try
                    {
                        await _estoqueService.RegistrarSaidaAsync(
                            registro.PecaEstoqueId.Value,
                            registro.Item.Quantidade,
                            $"Baixa para OS #{ordemServicoId}",
                            ordemServicoId);
                    }
                    catch
                    {
                        // Caso haja falha no estoque, apenas seguir; log poderia ser adicionado
                    }
                }
            }
        }

        private async Task<bool> ReporEstoqueAsync(int? ordemServicoId)
        {
            if (_estoqueService == null || !ordemServicoId.HasValue) return false;

            var movimentosSaida = await _context.MovimentacoesEstoque
                .Where(m => m.OrdemServicoId == ordemServicoId && m.Tipo == "Saida")
                .AsNoTracking()
                .ToListAsync();

            var devolvido = false;
            foreach (var mov in movimentosSaida)
            {
                try
                {
                    await _estoqueService.RegistrarEntradaAsync(
                        mov.PecaEstoqueId,
                        mov.Quantidade,
                        mov.ValorUnitario,
                        $"Devolução OS #{ordemServicoId}");
                    devolvido = true;
                }
                catch
                {
                }
            }

            return devolvido;
        }

        private async Task<List<PecaProcessada>> MontarPecaItensAsync(IEnumerable<PecaItemInput>? pecas)
        {
            var resultado = new List<PecaProcessada>();
            if (pecas == null) return resultado;

            var ativos = pecas.Where(p => p != null && p.Quantidade > 0 && (p.PecaEstoqueId.HasValue || !string.IsNullOrWhiteSpace(p.Nome))).ToList();
            if (!ativos.Any()) return resultado;

            var ids = ativos.Where(p => p.PecaEstoqueId.HasValue).Select(p => p.PecaEstoqueId!.Value).Distinct().ToList();
            var catalogo = ids.Count == 0
                ? new Dictionary<int, PecaEstoque>()
                : await _context.PecaEstoques.AsNoTracking().Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

            foreach (var entrada in ativos)
            {
                var nome = entrada.Nome;
                var valor = entrada.ValorUnitario;
                if (entrada.PecaEstoqueId.HasValue && catalogo.TryGetValue(entrada.PecaEstoqueId.Value, out var estoque))
                {
                    nome = estoque.Nome;
                    valor = estoque.PrecoVenda ?? valor;
                }

                resultado.Add(new PecaProcessada
                {
                    Item = new PecaItem
                    {
                        Nome = nome,
                        ValorUnitario = valor,
                        Quantidade = entrada.Quantidade,
                        PecaEstoqueId = entrada.PecaEstoqueId
                    },
                    PecaEstoqueId = entrada.PecaEstoqueId
                });
            }

            return resultado;
        }

        private IEnumerable<PecaProcessada> ConverterParaProcessadas(IEnumerable<PecaItem> pecas)
        {
            foreach (var item in pecas.Where(p => p.PecaEstoqueId.HasValue))
            {
                yield return new PecaProcessada
                {
                    Item = item,
                    PecaEstoqueId = item.PecaEstoqueId
                };
            }
        }

        private async Task<List<ApplicationUser>> GetMecanicosAsync()
        {
            var roleId = await _context.Roles
                .Where(r => r.Name == "Mecanico")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var mecanicos = await (from u in _context.Users
                                   join ur in _context.UserRoles on u.Id equals ur.UserId
                                   where ur.RoleId == roleId
                                   orderby u.NomeCompleto
                                   select u)
                                  .ToListAsync();

            return mecanicos;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Supervisor,Mecanico")]
        public async Task<IActionResult> ConcluirServicoItem(int id)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var item = await _context.ServicoItem
                .Include(s=>s.OrdemServico)
                .FirstOrDefaultAsync(s=>s.Id==id && s.OrdemServico.OficinaId == oficinaId);
            if (item == null) return NotFound();
            item.Concluido = true;
            await _context.SaveChangesAsync();
            await TentarConcluirOsSeTodosItens(item.OrdemServicoId);
            return RedirectToAction(nameof(Details), new { id = item.OrdemServicoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Supervisor,Mecanico")]
        public async Task<IActionResult> ConcluirPecaItem(int id)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var item = await _context.PecaItem
                .Include(p=>p.OrdemServico)
                .FirstOrDefaultAsync(p=>p.Id==id && p.OrdemServico.OficinaId == oficinaId);
            if (item == null) return NotFound();
            item.Concluido = true;
            await _context.SaveChangesAsync();
            await TentarConcluirOsSeTodosItens(item.OrdemServicoId);
            return RedirectToAction(nameof(Details), new { id = item.OrdemServicoId });
        }

        private async Task TentarConcluirOsSeTodosItens(int ordemId)
        {
            var os = await _context.OrdensServico
                .Include(o=>o.Servicos)
                .Include(o=>o.Pecas)
                .FirstOrDefaultAsync(o=>o.Id==ordemId);
            if (os == null) return;
            if (os.DataConclusao == null && (os.Servicos.All(s=>s.Concluido) && os.Pecas.All(p=>p.Concluido)))
            {
                os.DataConclusao = DateTime.UtcNow;
                os.Status = "Concluida";
                await _context.SaveChangesAsync();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GerarPDF(int id)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var os = await _context.OrdensServico
                .Include(o => o.Cliente)
                .Include(o => o.Veiculo)
                .Include(o => o.Mecanico)
                .Include(o => o.Servicos)
                .Include(o => o.Pecas)
                .FirstOrDefaultAsync(o => o.Id == id && o.OficinaId == oficinaId);
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

        [Authorize(Roles = "Mecanico,Admin,Supervisor")]
        public async Task<IActionResult> Minhas()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var oficinaId = await ObterOficinaAtualIdAsync();
            var lista = await _context.OrdensServico
                .Include(o => o.Veiculo)
                .Include(o => o.Servicos)
                .Where(o => o.OficinaId == oficinaId && o.MecanicoId == userId && o.DataConclusao == null && o.AprovadaCliente)
                .OrderBy(o => o.DataPrevista)
                .ToListAsync();

            return View(lista);
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

        private async Task GerarLancamentosFinanceirosDaOsAsync(OrdemServico ordem)
        {
            await _context.Entry(ordem).Collection(o => o.Servicos).LoadAsync();
            await _context.Entry(ordem).Collection(o => o.Pecas).LoadAsync();
            await _context.Entry(ordem).Reference(o => o.Oficina).LoadAsync();
            await _context.Entry(ordem).Reference(o => o.Cliente).LoadAsync();
            await _context.Entry(ordem).Reference(o => o.Mecanico).LoadAsync();

            if (ordem.Oficina == null) return;

            var totalServicos = ordem.Servicos?.Sum(s => s.Valor) ?? 0m;
            var totalPecas = ordem.Pecas?.Sum(p => p.ValorUnitario * p.Quantidade) ?? 0m;
            var totalOs = totalServicos + totalPecas;
            if (totalOs <= 0) return;

            var contaRecebimentoId = ordem.ContaRecebimentoId ?? await ObterContaPadraoAsync(ordem.OficinaId);
            if (contaRecebimentoId == 0) return;
            ordem.ContaRecebimentoId ??= contaRecebimentoId;

            var receitaCategoriaId = await ObterCategoriaPadraoAsync(ordem.OficinaId, FinanceiroTipoLancamento.Receita);
            var despesaCategoriaId = await ObterCategoriaPadraoAsync(ordem.OficinaId, FinanceiroTipoLancamento.Despesa);
            var primeiroVencimento = ordem.DataPrimeiroVencimento ?? DateTime.Today;
            var parcelas = ordem.FormaPagamento == FinanceiroFormaPagamento.CartaoCredito || ordem.FormaPagamento == FinanceiroFormaPagamento.Boleto
                ? Math.Max(1, ordem.QuantidadeParcelas)
                : 1;

            if (ordem.LancamentoFinanceiroReceitaId == null)
            {
                var lancReceita = new LancamentoFinanceiro
                {
                    OficinaId = ordem.OficinaId,
                    Tipo = FinanceiroTipoLancamento.Receita,
                    CategoriaFinanceiraId = receitaCategoriaId,
                    ContaPadraoId = contaRecebimentoId,
                    ClienteId = ordem.ClienteId,
                    Descricao = $"OS #{ordem.Id} - {(ordem.Cliente?.Nome ?? string.Empty)}",
                    NumeroDocumento = $"OS-{ordem.Id}",
                    DataCompetencia = DateTime.Today,
                    ValorTotal = totalOs,
                    FormaPagamento = ordem.FormaPagamento,
                    QuantidadeParcelas = parcelas,
                    Origem = $"OS#{ordem.Id}"
                };

                var baseValor = parcelas == 0 ? totalOs : decimal.Round(totalOs / parcelas, 2, MidpointRounding.AwayFromZero);
                for (var i = 0; i < parcelas; i++)
                {
                    var venc = primeiroVencimento.AddDays(30 * i);
                    var dias = (int)Math.Max(0, (venc - primeiroVencimento).TotalDays);
                    var valorParcela = baseValor;
                    if (dias > ordem.Oficina.FinanceiroPrazoSemJurosDias && ordem.Oficina.FinanceiroJurosMensal > 0)
                    {
                        var excedente = dias - ordem.Oficina.FinanceiroPrazoSemJurosDias;
                        var meses = Math.Ceiling(excedente / 30d);
                        var fator = Math.Pow(1 + (double)ordem.Oficina.FinanceiroJurosMensal, meses);
                        valorParcela = decimal.Round(baseValor * (decimal)fator, 2, MidpointRounding.AwayFromZero);
                    }

                    lancReceita.Parcelas.Add(new LancamentoParcela
                    {
                        Numero = i + 1,
                        DataVencimento = venc,
                        Valor = valorParcela,
                        Situacao = FinanceiroSituacaoParcela.Pendente
                    });
                }

                _context.LancamentosFinanceiros.Add(lancReceita);
                await _context.SaveChangesAsync();
                ordem.LancamentoFinanceiroReceitaId = lancReceita.Id;
            }

            if (totalPecas > 0 && ordem.LancamentoFinanceiroCustoPecasId == null)
            {
                var lancCusto = new LancamentoFinanceiro
                {
                    OficinaId = ordem.OficinaId,
                    Tipo = FinanceiroTipoLancamento.Despesa,
                    CategoriaFinanceiraId = despesaCategoriaId,
                    Descricao = $"Reposição de peças OS #{ordem.Id}",
                    NumeroDocumento = $"OS-PEC-{ordem.Id}",
                    DataCompetencia = DateTime.Today,
                    ValorTotal = totalPecas,
                    FormaPagamento = FinanceiroFormaPagamento.Dinheiro,
                    QuantidadeParcelas = 1,
                    Origem = $"OS#{ordem.Id}"
                };
                lancCusto.Parcelas.Add(new LancamentoParcela
                {
                    Numero = 1,
                    DataVencimento = DateTime.Today.AddDays(7),
                    Valor = totalPecas,
                    Situacao = FinanceiroSituacaoParcela.Pendente
                });
                _context.LancamentosFinanceiros.Add(lancCusto);
                await _context.SaveChangesAsync();
                ordem.LancamentoFinanceiroCustoPecasId = lancCusto.Id;
            }

            var comissaoPercentual = ordem.Mecanico?.PercentualComissao ?? 0m;
            if (comissaoPercentual > 0 && totalServicos > 0 && ordem.LancamentoFinanceiroComissaoId == null)
            {
                var valorComissao = decimal.Round(totalServicos * (comissaoPercentual / 100m), 2, MidpointRounding.AwayFromZero);
                var lancComissao = new LancamentoFinanceiro
                {
                    OficinaId = ordem.OficinaId,
                    Tipo = FinanceiroTipoLancamento.Despesa,
                    CategoriaFinanceiraId = despesaCategoriaId,
                    Descricao = $"Comissão mecânico OS #{ordem.Id}",
                    NumeroDocumento = $"OS-COM-{ordem.Id}",
                    DataCompetencia = DateTime.Today,
                    ValorTotal = valorComissao,
                    FormaPagamento = FinanceiroFormaPagamento.Dinheiro,
                    QuantidadeParcelas = 1,
                    Origem = $"OS#{ordem.Id}",
                    ClienteId = null,
                    ParceiroNome = ordem.Mecanico?.NomeCompleto
                };
                lancComissao.Parcelas.Add(new LancamentoParcela
                {
                    Numero = 1,
                    DataVencimento = DateTime.Today.AddDays(5),
                    Valor = valorComissao,
                    Situacao = FinanceiroSituacaoParcela.Pendente
                });
                _context.LancamentosFinanceiros.Add(lancComissao);
                await _context.SaveChangesAsync();
                ordem.LancamentoFinanceiroComissaoId = lancComissao.Id;
            }
        }

        private async Task<int> ObterCategoriaPadraoAsync(int oficinaId, FinanceiroTipoLancamento tipo)
        {
            var categoria = await _context.CategoriasFinanceiras
                .Where(c => c.OficinaId == oficinaId && c.Tipo == tipo)
                .OrderBy(c => c.Id)
                .FirstOrDefaultAsync();
            if (categoria == null)
            {
                throw new InvalidOperationException("Nenhuma categoria financeira configurada para a oficina.");
            }
            return categoria.Id;
        }

        private async Task<int> ObterContaPadraoAsync(int oficinaId)
        {
            var conta = await _context.ContasFinanceiras
                .Where(c => c.OficinaId == oficinaId)
                .OrderBy(c => c.Id)
                .FirstOrDefaultAsync();
            return conta?.Id ?? 0;
        }

        private async Task PopularDadosFormularioAsync(int oficinaId, int? clienteId = null, string? mecanicoId = null, int? veiculoId = null, int? contaRecebimentoId = null)
        {
            var clientes = await _context.OficinasClientes
                .Where(oc => oc.OficinaId == oficinaId)
                .Select(oc => oc.Cliente)
                .OrderBy(c => c.Nome)
                .ToListAsync();
            ViewData["ClienteId"] = new SelectList(clientes, "Id", "Nome", clienteId);

            var mecanicos = await GetMecanicosAsync();
            ViewData["MecanicoId"] = new SelectList(mecanicos, "Id", "NomeCompleto", mecanicoId);

            var veiculos = await _context.Veiculos
                .Where(v => v.Oficinas.Any(o => o.OficinaId == oficinaId))
                .OrderBy(v => v.Placa)
                .ToListAsync();
            ViewData["VeiculoId"] = new SelectList(veiculos, "Id", "Placa", veiculoId);

            ViewBag.PecasEstoque = await _context.PecaEstoques
                .AsNoTracking()
                .Where(p => p.OficinaId == oficinaId)
                .OrderBy(p => p.Nome)
                .ToListAsync();

            var contas = await _context.ContasFinanceiras
                .Where(c => c.OficinaId == oficinaId)
                .OrderBy(c => c.Nome)
                .ToListAsync();
            ViewData["ContaRecebimentoId"] = new SelectList(contas, "Id", "Nome", contaRecebimentoId);
        }
    }
}


