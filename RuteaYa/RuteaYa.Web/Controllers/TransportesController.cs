using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Entidades;
using RuteaYa.Logica.Transportes;
using RuteaYa.Logica.Transportes.Dtos;

namespace RuteaYa.Web.Controllers
    {

    public class TransportesController : Controller
        {

        private readonly ITransporteServicios _transporteServicios;
        private readonly UserManager<Usuario> _userManager;

        public TransportesController(ITransporteServicios transporteServicios, UserManager<Usuario> userManager)
            {
                this._transporteServicios = transporteServicios;
                this._userManager = userManager;
            }

        [HttpGet]
        public async Task<IActionResult> Index()
            {

            var usuario = await this._userManager.GetUserAsync(User);

            if (usuario == null)
                {
                return Challenge();
                }

                var transportes = await this._transporteServicios.ListarMisTransportesAsync(usuario.Id);
                return View(transportes);
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarRetirado(int envioId)
            {

            var usuario = await this._userManager.GetUserAsync(User);
            if (usuario == null)
                {
                return Challenge();
                }

                var resultado = await this._transporteServicios.MarcarRetiradoAsync(usuario.Id, envioId);
                ProcesarResultado(resultado, "El paquete fue marcado como retirado.");
                return RedirectToAction(nameof(Index));
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarEnCamino(int envioId)
            {
            var usuario = await this._userManager.GetUserAsync(User);
            if (usuario == null)
                {
                return Challenge();
                }

                var resultado = await this._transporteServicios.MarcarEnCaminoAsync(usuario.Id, envioId);
                ProcesarResultado(resultado, "El paquete ahora está en camino a destino.");
                return RedirectToAction(nameof(Index));
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarEntregado(int envioId)
            {

            var usuario = await this._userManager.GetUserAsync(User);
            if (usuario == null)
                {
                return Challenge();
                }

                var resultado = await this._transporteServicios.MarcarEntregadoAsync(usuario.Id, envioId);
                ProcesarResultado(resultado, "¡Paquete entregado con éxito!, se completó el transporte.");
                return RedirectToAction(nameof(Index));
            }

        private void ProcesarResultado(ResultadoActualizarTransporte resultado, string mensajeExito)
            {
            if (resultado.ResultadoExitoso)
                {
                TempData["MensajeExito"] = mensajeExito;
                }
            else if (resultado.Error != null)
                {
                TempData["MensajeError"] = resultado.Error;
                }
            else
                {
                TempData["MensajeError"] = "No se pudo actualizar el estado del transporte.";
                }
            }
        }
    }