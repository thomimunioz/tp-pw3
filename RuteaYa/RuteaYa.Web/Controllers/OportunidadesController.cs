using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Logica.Asignaciones;
using RuteaYa.Logica.Oportunidades;
using RuteaYa.Logica.Oportunidades.Dtos;
using RuteaYa.Logica.Viajes;
using RuteaYa.Web.Models;

namespace RuteaYa.Web.Controllers
{
    public class OportunidadesController : Controller
    {
        private readonly IOportunidadesServicios _oportunidades;
        private readonly IAsignacionesServicios _asignaciones;
        private readonly IViajeServicios _viajes;

        public OportunidadesController(IOportunidadesServicios oportunidades,
                                       IAsignacionesServicios asignaciones,
                                       IViajeServicios viajes)
        {
            _oportunidades = oportunidades;
            _asignaciones = asignaciones;
            _viajes = viajes;
        }

        private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> Index(int viajeId, OrdenOportunidades orden = OrdenOportunidades.Ganancia)
        {
            var viaje = await _viajes.ObtenerDetallePorUsuarioAsync(viajeId, UsuarioId);
            var lista = await _oportunidades.BuscarAsync(UsuarioId, viajeId);

            if (viaje == null || lista == null)
                return NotFound();

            var modelo = new OportunidadesIndexViewModel
            {
                ViajeId = viajeId,
                Titulo = $"{viaje.OrigenDir} → {viaje.DestinoDir}",
                Orden = orden,
                Oportunidades = OrdenadorOportunidades.Ordenar(lista, orden)
            };

            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int viajeId, int envioId)
        {
            var detalle = await _oportunidades.ObtenerDetalleAsync(UsuarioId, viajeId, envioId);

            if (detalle == null)
            {
                TempData["Error"] = "Este envío ya no está disponible para tu viaje.";
                return RedirectToAction(nameof(Index), new { viajeId });
            }

            return View(new DetalleOportunidadViewModel { ViajeId = viajeId, Oportunidad = detalle });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aceptar(int viajeId, int envioId)
        {
            var resultado = await _asignaciones.AceptarAsync(UsuarioId, viajeId, envioId);

            switch (resultado)
            {
                case ResultadoAceptar.Exito:
                    TempData["Exito"] = "Envío aceptado. Ya figura entre tus transportes.";
                    break;
                case ResultadoAceptar.YaTomado:
                    TempData["Error"] = "Otro conductor tomó este envío antes que vos.";
                    break;
                case ResultadoAceptar.NoCumple:
                    TempData["Error"] = "El envío dejó de cumplir las condiciones de tu viaje (peso, cantidad, desvío o fecha).";
                    break;
                default:
                    return NotFound();
            }

            return RedirectToAction(nameof(Index), new { viajeId });
        }
    }
}