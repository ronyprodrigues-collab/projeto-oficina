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
    [Authorize(Roles = "SuporteTecnico,Admin,Diretor")]
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
            var query = _context.Grupos
                .Include(g => g.Diretor)
                .Include(g => g.Oficinas)
                .AsQueryable();

            if (UsuarioDiretorRestrito)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Forbid();
                query = query.Where(g => g.DiretorId == user.Id);
            }

            var grupos = await query
                .AsNoTracking()
                .OrderBy(g => g.Nome)
                .ToListAsync();
            return View(grupos);
        }

        public async Task<IActionResult> Create(string? diretorId)
        {
            await PopularDiretoresAsync(diretorId);
            return View(new GrupoComOficinaViewModel
            {
                Plano = PlanoConta.Basico,
                CorPrimaria = "#0d6efd",
                CorSecundaria = "#6c757d",
                OficinaPlano = PlanoConta.Basico,
                CriarOficinaInicial = true,
                DiretorId = diretorId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GrupoComOficinaViewModel model)
        {
            if (model.CriarOficinaInicial && string.IsNullOrWhiteSpace(model.OficinaNome))
            {
                ModelState.AddModelError(nameof(model.OficinaNome), "Informe o nome da primeira oficina.");
            }

            var nomeGrupo = (model.Nome ?? string.Empty).Trim();
            if (await _context.Grupos.AnyAsync(g => g.Nome == nomeGrupo))
            {
                ModelState.AddModelError(nameof(model.Nome), "Já existe um grupo com este nome.");
            }

            if (!ModelState.IsValid)
            {
                await PopularDiretoresAsync(model.DiretorId);
                return View(model);
            }

            var grupo = new GrupoOficina
            {
                Nome = nomeGrupo,
                Descricao = model.Descricao,
                DiretorId = model.DiretorId ?? string.Empty,
                Plano = model.Plano,
                CorPrimaria = string.IsNullOrWhiteSpace(model.CorPrimaria) ? "#0d6efd" : model.CorPrimaria,
                CorSecundaria = string.IsNullOrWhiteSpace(model.CorSecundaria) ? "#6c757d" : model.CorSecundaria
            };

            _context.Grupos.Add(grupo);
            await _context.SaveChangesAsync();

            if (model.CriarOficinaInicial && !string.IsNullOrWhiteSpace(model.OficinaNome))
            {
                var oficina = new Oficina
                {
                    Nome = model.OficinaNome,
                    Descricao = model.OficinaDescricao,
                    GrupoOficinaId = grupo.Id,
                    Plano = model.OficinaPlano,
                    CorPrimaria = grupo.CorPrimaria,
                    CorSecundaria = grupo.CorSecundaria,
                    FinanceiroPrazoSemJurosDias = 90,
                    FinanceiroJurosMensal = 0.02m
                };
                _context.Oficinas.Add(oficina);
                await _context.SaveChangesAsync();
                await CriarRecursosFinanceirosPadraoAsync(oficina);
            }

            await GarantirDiretorRoleAsync(grupo.DiretorId);

            TempData["Msg"] = "Grupo criado com sucesso.";
            return RedirectToAction(nameof(Index));
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

            if (UsuarioDiretorRestrito)
            {
                var current = await _userManager.GetUserAsync(User);
                if (current != null)
                {
                    diretores = diretores.Where(d => d.Id == current.Id).ToList();
                }
            }

            ViewBag.Diretores = new SelectList(diretores, "Id", "Nome", selecionado);
        }

        private async Task GarantirDiretorRoleAsync(string? diretorId)
        {
            if (string.IsNullOrWhiteSpace(diretorId)) return;
            var diretor = await _userManager.FindByIdAsync(diretorId);
            if (diretor == null) return;
            if (!await _userManager.IsInRoleAsync(diretor, "Diretor"))
            {
                await _userManager.AddToRoleAsync(diretor, "Diretor");
            }
            diretor.Cargo = string.IsNullOrWhiteSpace(diretor.Cargo) ? "Diretor" : diretor.Cargo;
            await _userManager.UpdateAsync(diretor);
        }

        private bool UsuarioDiretorRestrito =>
            User.IsInRole("Diretor") &&
            !User.IsInRole("SuporteTecnico") &&
            !User.IsInRole("Admin");

        private async Task CriarRecursosFinanceirosPadraoAsync(Oficina oficina)
        {
            if (!await _context.ContasFinanceiras.AnyAsync(c => c.OficinaId == oficina.Id))
            {
                _context.ContasFinanceiras.AddRange(
                    new ContaFinanceira
                    {
                        OficinaId = oficina.Id,
                        Nome = "Caixa Principal",
                        Tipo = FinanceiroTipoConta.Caixa,
                        SaldoInicial = 0,
                        Ativo = true
                    },
                    new ContaFinanceira
                    {
                        OficinaId = oficina.Id,
                        Nome = "Banco Padrão",
                        Tipo = FinanceiroTipoConta.Banco,
                        Banco = "000",
                        Agencia = "0000",
                        NumeroConta = "000000-0",
                        SaldoInicial = 0,
                        Ativo = true
                    });
                await _context.SaveChangesAsync();
            }

            if (!await _context.CategoriasFinanceiras.AnyAsync(c => c.OficinaId == oficina.Id))
            {
                _context.CategoriasFinanceiras.AddRange(
                    new CategoriaFinanceira
                    {
                        OficinaId = oficina.Id,
                        Nome = "Serviços de Oficina",
                        Tipo = FinanceiroTipoLancamento.Receita,
                        Descricao = "Receitas geradas por ordens de serviço."
                    },
                    new CategoriaFinanceira
                    {
                        OficinaId = oficina.Id,
                        Nome = "Compra de Peças",
                        Tipo = FinanceiroTipoLancamento.Despesa,
                        Descricao = "Reposição de estoque e insumos."
                    },
                    new CategoriaFinanceira
                    {
                        OficinaId = oficina.Id,
                        Nome = "Comissões",
                        Tipo = FinanceiroTipoLancamento.Despesa,
                        Descricao = "Pagamentos de comissões para equipe."
                    });
                await _context.SaveChangesAsync();
            }
        }
    }
}
