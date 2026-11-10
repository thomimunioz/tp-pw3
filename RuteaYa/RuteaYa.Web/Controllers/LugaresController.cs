using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Logica.Lugares;

namespace RuteaYa.Web.Controllers;

/// <summary>Endpoint que usa el JS para sugerir lugares. No tiene lógica: solo le pregunta al servicio.</summary>
[Authorize]
public class LugaresController : Controller
{
    private readonly ISugeridorLugares _sugeridor;

    public LugaresController(ISugeridorLugares sugeridor)
    {
        _sugeridor = sugeridor;
    }

    // GET /Lugares/Sugerir?q=cordo
    [HttpGet]
    public async Task<IActionResult> Sugerir(string? q, CancellationToken cancellationToken)
    {
        var lugares = await _sugeridor.BuscarAsync(q ?? string.Empty, cancellationToken);
        return Json(lugares);
    }
}