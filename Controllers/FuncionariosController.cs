using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;

namespace projetos.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FuncionariosController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public FuncionariosController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var supervisores = await _userManager.GetUsersInRoleAsync("Supervisor");
            var mecanicos = await _userManager.GetUsersInRoleAsync("Mecanico");
            var lista = supervisores.Concat(mecanicos).Distinct().ToList();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateFuncionarioViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateFuncionarioViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var cargoNormalizado = (model.Cargo ?? string.Empty).Trim();
            if (cargoNormalizado != "Supervisor" && cargoNormalizado != "Mecanico")
            {
                ModelState.AddModelError("Cargo", "Selecione Supervisor ou Mecanico.");
                return View(model);
            }

            var exists = await _userManager.FindByEmailAsync(model.Email);
            if (exists != null)
            {
                ModelState.AddModelError("Email", "Já existe um usuário com este e-mail.");
                return View(model);
            }

            // Garante que a role exista
            if (!await _roleManager.RoleExistsAsync(cargoNormalizado))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(cargoNormalizado));
                if (!roleResult.Succeeded)
                {
                    foreach (var e in roleResult.Errors) ModelState.AddModelError(string.Empty, e.Description);
                    return View(model);
                }
            }

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                NomeCompleto = model.NomeCompleto,
                Cargo = cargoNormalizado,
                EmailConfirmed = true
            };
            var createRes = await _userManager.CreateAsync(user, string.IsNullOrWhiteSpace(model.Senha) ? "P@ssw0rd!" : model.Senha);
            if (!createRes.Succeeded)
            {
                foreach (var e in createRes.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }
            await _userManager.AddToRoleAsync(user, cargoNormalizado);
            TempData["Msg"] = $"Funcionário '{user.NomeCompleto}' criado como {cargoNormalizado}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var roles = await _userManager.GetRolesAsync(user);
            var cargo = roles.Contains("Supervisor") ? "Supervisor" : (roles.Contains("Mecanico") ? "Mecanico" : (user.Cargo ?? string.Empty));
            var vm = new EditFuncionarioViewModel
            {
                Id = user.Id,
                NomeCompleto = user.NomeCompleto ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Cargo = cargo,
                Ativo = !(user.LockoutEnd.HasValue && user.LockoutEnd.Value.UtcDateTime > System.DateTime.UtcNow)
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditFuncionarioViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.NomeCompleto = model.NomeCompleto;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.Cargo = model.Cargo;

            var upd = await _userManager.UpdateAsync(user);
            if (!upd.Succeeded)
            {
                foreach (var e in upd.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            // Ajusta cargo (role)
            var desired = (model.Cargo ?? string.Empty).Trim();
            if (desired != "Supervisor" && desired != "Mecanico")
            {
                ModelState.AddModelError("Cargo", "Selecione Supervisor ou Mecanico.");
                return View(model);
            }
            if (!await _roleManager.RoleExistsAsync(desired))
            {
                await _roleManager.CreateAsync(new IdentityRole(desired));
            }
            var currentRoles = await _userManager.GetRolesAsync(user);
            var toRemove = currentRoles.Where(r => r == "Supervisor" || r == "Mecanico").ToList();
            if (toRemove.Any()) await _userManager.RemoveFromRolesAsync(user, toRemove);
            await _userManager.AddToRoleAsync(user, desired);

            // Ajusta status (ativo/inativo) via lockout
            var activeNow = !(user.LockoutEnd.HasValue && user.LockoutEnd.Value.UtcDateTime > System.DateTime.UtcNow);
            if (model.Ativo != activeNow)
            {
                if (!model.Ativo)
                {
                    user.LockoutEnabled = true;
                    user.LockoutEnd = System.DateTimeOffset.UtcNow.AddYears(100);
                }
                else
                {
                    user.LockoutEnd = null;
                }
                await _userManager.UpdateAsync(user);
            }

            // Troca de senha, se fornecida
            if (!string.IsNullOrWhiteSpace(model.NovaSenha))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var res = await _userManager.ResetPasswordAsync(user, token, model.NovaSenha);
                if (!res.Succeeded)
                {
                    foreach (var e in res.Errors) ModelState.AddModelError(string.Empty, e.Description);
                    return View(model);
                }
            }

            TempData["Msg"] = $"Funcionário '{user.NomeCompleto}' atualizado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAtivo(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var ativo = !(user.LockoutEnd.HasValue && user.LockoutEnd.Value.UtcDateTime > System.DateTime.UtcNow);
            if (ativo)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = System.DateTimeOffset.UtcNow.AddYears(100);
                TempData["Msg"] = $"Funcionário '{user.NomeCompleto}' desativado.";
            }
            else
            {
                user.LockoutEnd = null;
                TempData["Msg"] = $"Funcionário '{user.NomeCompleto}' reativado.";
            }
            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }
}
