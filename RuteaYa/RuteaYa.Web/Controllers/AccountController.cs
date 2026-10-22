using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Entidades;
using RuteaYa.Logica.Account;
using RuteaYa.Web.Models;

namespace RuteaYa.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountServicios _accountServicios;
        private readonly SignInManager<Usuario> _signInManager;
        public AccountController(IAccountServicios accountServicios,
                             SignInManager<Usuario> signInManager)
        {
            _accountServicios = accountServicios;
            _signInManager = signInManager;
        }

        //registro
        [AllowAnonymous]
        public IActionResult Registro()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var resultado = await _accountServicios.RegistrarAsync(
            model.Nombre, model.Apellido, model.Email, model.Password);

            if (resultado.Exito)
            {
                await _signInManager.SignInAsync(resultado.Usuario!, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in resultado.Errores)
                ModelState.AddModelError(string.Empty, error);

            return View(model);
        }
        //login
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = await _accountServicios.ValidarCredencialesAsync(model.Email, model.Password);

            if (usuario != null)
            {
                await _signInManager.SignInAsync(usuario, model.Recordarme);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos");
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
