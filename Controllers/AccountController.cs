using System.Linq;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;
using Services;

namespace projetos.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOficinaContext _oficinaContext;
        private readonly OficinaDbContext _context;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IOficinaContext oficinaContext, OficinaDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _oficinaContext = oficinaContext;
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Senha, isPersistent: true, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _oficinaContext.Clear();

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return RedirectToAction("Selecionar", "Oficinas", new { returnUrl = model.ReturnUrl });

                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("SuporteTecnico"))
                    return RedirectToAction("Index", "Suporte");

                string defaultController = "Painel";
                string defaultAction = "Index";
                if (roles.Contains("Mecanico"))
                {
                    defaultController = "OrdensServico";
                    defaultAction = "Minhas";
                }

                var url = Url.Action(defaultAction, defaultController) ?? "/Painel";
                return RedirectToAction("Selecionar", "Oficinas", new { returnUrl = url });
            }

            ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register(string? token = null)
        {
            return View(new RegisterViewModel { Token = token });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                NomeCompleto = model.NomeCompleto,
                Cargo = model.Cargo,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Senha);
            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(model.Token))
                {
                    var convite = await _context.Convites.FirstOrDefaultAsync(c => c.Token == model.Token && !c.Usado);
                    if (convite != null)
                    {
                        await _userManager.AddToRoleAsync(user, convite.PerfilDestino);
                        user.PercentualComissao = convite.PercentualComissao;
                        convite.Usado = true;
                        await _context.SaveChangesAsync();
                    }
                }

                await _signInManager.SignInAsync(user, isPersistent: true);
                _oficinaContext.Clear();
                var url = Url.Action("Index", "Painel") ?? "/Painel";
                return RedirectToAction("Selecionar", "Oficinas", new { returnUrl = url });
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _oficinaContext.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}


