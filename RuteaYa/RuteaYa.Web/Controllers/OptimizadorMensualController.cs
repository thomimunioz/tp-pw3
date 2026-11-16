using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Logica.Optimizacion;

namespace RuteaYa.Web.Controllers;

public class OptimizadorMensualController : Controller
{
    private readonly IProyeccionMensualServicios _proyeccionMensual;

    public OptimizadorMensualController(IProyeccionMensualServicios proyeccionMensual)
    {
        _proyeccionMensual = proyeccionMensual;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? mes,
        CriterioOptimizacion criterio = CriterioOptimizacion.MaximizarGanancias)
    {
        if (!ModelState.IsValid || !Enum.IsDefined(criterio))
        {
            return BadRequest(ModelState);
        }

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var mesActual = new DateOnly(hoy.Year, hoy.Month, 1);
        var mesSeleccionado = mesActual;

        if (!string.IsNullOrWhiteSpace(mes))
        {
            if (!DateTime.TryParseExact(
                mes,
                "yyyy-MM",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var fechaMes))
            {
                TempData["Error"] = "El mes seleccionado no es válido.";
            }
            else
            {
                mesSeleccionado = new DateOnly(fechaMes.Year, fechaMes.Month, 1);

                if (mesSeleccionado < mesActual)
                {
                    TempData["Error"] = "La proyección solo está disponible para el mes actual o uno futuro.";
                    mesSeleccionado = mesActual;
                }
            }
        }

        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var proyeccion = await _proyeccionMensual.CalcularAsync(
            usuarioId,
            mesSeleccionado,
            criterio);

        return View(proyeccion);
    }
}
