using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Entidades;
using RuteaYa.Web.Models;

namespace RuteaYa.Web.Controllers
{
    public class AccountController : Controller
    {
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
        public AccountController(UserManager<Usuario> userManager,
                          SignInManager<Usuario> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Registro()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = new Usuario
            {
                UserName = model.Email,
                Email = model.Email,
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                CreadoEn = DateTime.UtcNow
            };

            var resultado = await _userManager.CreateAsync(usuario, model.Password);

            if (resultado.Succeeded)
            {
                await _signInManager.SignInAsync(usuario, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in resultado.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
