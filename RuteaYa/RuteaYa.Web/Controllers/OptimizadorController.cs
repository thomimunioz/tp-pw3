using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Logica.Asignaciones;
using RuteaYa.Logica.Optimizacion;
using RuteaYa.Logica.Viajes;
using RuteaYa.Web.Models;

namespace RuteaYa.Web.Controllers;

public class OptimizadorController : Controller
{
    private readonly IOptimizadorServicios _optimizador;
    private readonly IAsignacionesServicios _asignaciones;
    private readonly IViajeSelectorServicios _selector;

    public OptimizadorController(IOptimizadorServicios optimizador,
                                 IAsignacionesServicios asignaciones,
                                 IViajeSelectorServicios selector)
    {
        _optimizador = optimizador;
        _asignaciones = asignaciones;
        _selector = selector;
    }

    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Pantalla 10: combinación recomendada, alternativas y descartadas
    [HttpGet]
    public async Task<IActionResult> Index(int? viajeId)
    {
        var viajes = await _selector.ObtenerActivosAsync(UsuarioId);

        if (viajes.Count == 0)
            return View(new OptimizadorViewModel());

        var id = viajeId ?? await _selector.ObtenerViajeIdPorDefectoAsync(UsuarioId, DateTime.Now);
        if (id == null)
            return NotFound();

        var optimizacion = await _optimizador.CalcularAsync(UsuarioId, id.Value);
        if (optimizacion == null)
            return NotFound();

        return View(new OptimizadorViewModel
        {
            ViajeId = id.Value,
            Viajes = viajes,
            Optimizacion = optimizacion
        });
    }

    // Pantalla 11: resumen antes de reservar
    [HttpGet]
    public async Task<IActionResult> Confirmar(int viajeId)
    {
        var optimizacion = await _optimizador.CalcularAsync(UsuarioId, viajeId);
        if (optimizacion == null)
            return NotFound();

        if (optimizacion.Resultado.Seleccionados.Count == 0)
        {
            TempData["Error"] = "Ya no hay una combinación para confirmar en este viaje.";
            return RedirectToAction(nameof(Index), new { viajeId });
        }

        return View(new OptimizadorViewModel { ViajeId = viajeId, Optimizacion = optimizacion });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reservar(int viajeId, List<int> envioIds)
    {
        var resultado = await _asignaciones.ConfirmarCombinacionAsync(UsuarioId, viajeId, envioIds);

        switch (resultado)
        {
            case ResultadoAceptar.Exito:
                TempData["MensajeExito"] = $"Reservaste {envioIds.Count} envío(s). Ya figuran en tus transportes.";
                return RedirectToAction("Index", "Transportes");
            case ResultadoAceptar.YaTomado:
                TempData["Error"] = "Otro conductor tomó uno de los envíos. Te mostramos la combinación recalculada.";
                break;
            case ResultadoAceptar.NoCumple:
                TempData["Error"] = "La combinación dejó de cumplir las condiciones del viaje. Te mostramos la combinación recalculada.";
                break;
            default:
                return NotFound();
        }

        return RedirectToAction(nameof(Index), new { viajeId });
    }
}
