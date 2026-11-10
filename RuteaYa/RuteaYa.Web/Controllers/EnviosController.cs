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
        public async Task<IActionResult> Index(EstadoEnvio? estado)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return Challenge();
            }

            // si alguien escribe a mano ?estado=99 en la URL, lo tratamos como "Todos".
            if (estado.HasValue && Enum.IsDefined(typeof(EstadoEnvio), estado.Value) == false)
            {
                estado = null;
            }

            var envios = await _envioServicios.ListarAsync(usuario.Id, estado);

            var model = new MisEnviosViewModel
            {
                EstadoSeleccionado = estado,
                Envios = envios
            };

            return View(model);
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
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return Challenge();
            }

            var envio = await _envioServicios.ObtenerDetalleAsync(usuario.Id, id);
            if (envio == null)
            {
                return NotFound();
            }

            return View(envio);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                return Challenge();
            }

            var resultado = await _envioServicios.CancelarAsync(usuario.Id, id);

            if (resultado.NoEncontrado)
            {
                return NotFound();
            }

            if (resultado.ResultadoExitoso)
            {
                TempData["MensajeExito"] = "El envío fue cancelado.";
            }
            else
            {
                TempData["MensajeError"] = resultado.Error;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}