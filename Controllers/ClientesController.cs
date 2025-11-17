using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Data;
using Models;
using Services;
using Models.ViewModels;

namespace projetos.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class ClientesController : Controller
    {
        private readonly OficinaDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOficinaContext _oficinaContext;

        public ClientesController(OficinaDbContext context, UserManager<ApplicationUser> userManager, IOficinaContext oficinaContext)
        {
            _context = context;
            _userManager = userManager;
            _oficinaContext = oficinaContext;
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var clientes = await _context.OficinasClientes
                .Where(oc => oc.OficinaId == oficinaId)
                .Select(oc => oc.Cliente)
                .Include(c => c.Veiculos)
                .Include(c => c.Oficinas)
                    .ThenInclude(oc => oc.Oficina)
                .AsNoTracking()
                .ToListAsync();
            return View(clientes);
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var oficinaId = await ObterOficinaAtualIdAsync();
            var cliente = await _context.Clientes
                .Include(c => c.Veiculos)
                .FirstOrDefaultAsync(m => m.Id == id && m.Oficinas.Any(o => o.OficinaId == oficinaId));

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        // GET: Clientes/Create
        public async Task<IActionResult> Create()
        {
            var responsavel = await ObterNomeResponsavelAsync();
            var model = new Cliente
            {
                Responsavel = responsavel
            };
            return View(model);
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(
            "Id,Nome,CPF_CNPJ,Telefone,Email,Endereco," +
            "Numero,Bairro,Cidade,Estado,CEP," +
            "DataNascimento,Observacoes,TipoCliente,CNPJ,Responsavel"
        )] Cliente cliente)
        {
            if (string.IsNullOrWhiteSpace(cliente.Responsavel))
            {
                cliente.Responsavel = await ObterNomeResponsavelAsync();
            }

            if (ModelState.IsValid)
            {
                var oficinaId = await ObterOficinaAtualIdAsync();
                _context.Add(cliente);
                await _context.SaveChangesAsync();

                _context.OficinasClientes.Add(new OficinaCliente
                {
                    ClienteId = cliente.Id,
                    OficinaId = oficinaId,
                    VinculadoEm = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(cliente);
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var oficinaId = await ObterOficinaAtualIdAsync();
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id && c.Oficinas.Any(o => o.OficinaId == oficinaId));

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(
            "Id,Nome,CPF_CNPJ,Telefone,Email,Endereco," +
            "Numero,Bairro,Cidade,Estado,CEP," +
            "DataNascimento,Observacoes,TipoCliente,CNPJ,Responsavel"
        )] Cliente cliente)
        {
            if (id != cliente.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var oficinaId = await ObterOficinaAtualIdAsync();
                    var original = await _context.Clientes
                        .FirstOrDefaultAsync(c => c.Id == id && c.Oficinas.Any(o => o.OficinaId == oficinaId));
                    if (original == null)
                    {
                        return NotFound();
                    }

                    _context.Entry(original).CurrentValues.SetValues(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(cliente);
        }

        private async Task<string> ObterNomeResponsavelAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return User?.Identity?.Name ?? string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(user.NomeCompleto))
            {
                return user.NomeCompleto;
            }

            return user.Email ?? user.UserName ?? string.Empty;
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var oficinaId = await ObterOficinaAtualIdAsync();
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.Id == id && m.Oficinas.Any(o => o.OficinaId == oficinaId));

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id && c.Oficinas.Any(o => o.OficinaId == oficinaId));

            if (cliente != null)
                _context.Clientes.Remove(cliente);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<int> ObterOficinaAtualIdAsync()
        {
            var oficina = await _oficinaContext.GetOficinaAtualAsync();
            if (oficina == null) throw new InvalidOperationException("Nenhuma oficina selecionada.");
            return oficina.Id;
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Vincular(string? busca)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var query = _context.Clientes
                .Where(c => !c.Oficinas.Any(o => o.OficinaId == oficinaId));

            if (!string.IsNullOrWhiteSpace(busca))
            {
                query = query.Where(c =>
                    c.Nome.Contains(busca) ||
                    (c.CPF_CNPJ != null && c.CPF_CNPJ.Contains(busca)) ||
                    (c.CNPJ != null && c.CNPJ.Contains(busca)));
            }

            var candidatos = await query
                .Select(c => new
                {
                    c.Id,
                    c.Nome,
                    Documento = c.CNPJ ?? c.CPF_CNPJ ?? string.Empty,
                    Origem = c.Oficinas.Select(o => o.Oficina.Nome)
                })
                .OrderBy(c => c.Nome)
                .ToListAsync();

            var viewModel = new VincularClienteViewModel
            {
                Busca = busca,
                Clientes = candidatos
                    .Select(c => new ClienteDisponivelViewModel
                    {
                        Id = c.Id,
                        Nome = c.Nome,
                        Documento = c.Documento,
                        Origem = c.Origem.Any() ? string.Join(", ", c.Origem) : "Sem origem definida"
                    }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VincularCliente(int clienteId)
        {
            var oficinaId = await ObterOficinaAtualIdAsync();
            var existe = await _context.OficinasClientes
                .AnyAsync(oc => oc.OficinaId == oficinaId && oc.ClienteId == clienteId);
            if (existe)
            {
                TempData["Error"] = "Cliente já está disponível nesta oficina.";
                return RedirectToAction(nameof(Index));
            }

            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                TempData["Error"] = "Cliente não encontrado.";
                return RedirectToAction(nameof(Vincular));
            }

            _context.OficinasClientes.Add(new OficinaCliente
            {
                ClienteId = clienteId,
                OficinaId = oficinaId,
                VinculadoEm = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["Msg"] = "Cliente vinculado à oficina.";
            return RedirectToAction(nameof(Index));
        }
    }
}
