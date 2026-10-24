using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Entidades;
using RuteaYa.Logica.Envios;
using RuteaYa.Logica.Envios.Dtos;
using RuteaYa.Web.Models.Envios;

namespace RuteaYa.Web.Controllers {

    public class EnviosController : Controller
    {

        private readonly IEnvioServicios _envioServicios;
        private readonly UserManager<Usuario> _userManager;

        public EnviosController(IEnvioServicios envioServicios, UserManager<Usuario> userManager)
        {
            _envioServicios = envioServicios;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Crear));
        }

        [HttpGet]
        public IActionResult Crear()
        {
            var model = new CrearEnvioViewModel
            {
                FechaLimite = DateOnly.FromDateTime(DateTime.Now)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CrearEnvioViewModel model)
        {

            if (model.OrigenDirrecion.Trim().Equals(model.DestinoDireccion.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.DestinoDireccion), "El destino debe ser diferente al origen.");
            }

            if (model.FechaLimite < DateOnly.FromDateTime(DateTime.Now))
            {
                ModelState.AddModelError(nameof(model.FechaLimite), "La fecha no puede ser anterior a la actual.");
            }

            if (ModelState.IsValid == false)
            {
                return View(model);
            }

            // Obtener el usuario que inició sesión actualmente.
            var usuario = await _userManager.GetUserAsync(User);
            if(usuario == null)
            {
                return Challenge();
            }

            var dto = new CrearEnvioDto
            {
                OrigenDirrecion = model.OrigenDirrecion,
                DestinoDireccion = model.DestinoDireccion,
                FechaLimite = model.FechaLimite,
                Peso = model.Peso,
                Monto = model.Monto,
                Tamano = model.tamano,
                Descripcion = model.Descripcion
            };

            var resultado = await _envioServicios.CrearAsync(usuario.Id, dto);
            if(resultado.ResultadoExitoso == false)
            {
                foreach(var error in resultado.Errores)
                {
                    ModelState.AddModelError("", error);
                }

                return View(model);
            }

            TempData["MensajeExito"] = "¡Tu envío fue publicado con éxito y ya está disponible buscando conductor!.";
            return RedirectToAction("Index", "Home");
        }
    }
}