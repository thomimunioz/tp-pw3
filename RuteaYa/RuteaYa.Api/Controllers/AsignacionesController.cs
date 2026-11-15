using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RuteaYa.Api.Dtos;
using RuteaYa.Logica.Asignaciones;

namespace RuteaYa.Api.Controllers;

[ApiController]
[Route("api/viajes/{viajeId:int}/asignaciones")]
public class AsignacionesController : ControllerBase
{
    private readonly IAsignacionesServicios _asignaciones;

    public AsignacionesController(IAsignacionesServicios asignaciones) => _asignaciones = asignaciones;

    // POST api/viajes/5/asignaciones/8?usuarioId=2
    [HttpPost("{envioId:int}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Aceptar(int viajeId, int envioId, [FromQuery, BindRequired] int usuarioId)
    {
        var resultado = await _asignaciones.AceptarAsync(usuarioId, viajeId, envioId);
        return Respuesta(resultado);
    }

    // POST api/viajes/5/asignaciones?usuarioId=2 con body { "envioIds": [8, 9] }
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ConfirmarCombinacion(int viajeId, [FromQuery, BindRequired] int usuarioId,
        [FromBody] ConfirmarCombinacionRequest request)
    {
        var resultado = await _asignaciones.ConfirmarCombinacionAsync(usuarioId, viajeId, request.EnvioIds);
        return Respuesta(resultado);
    }

    // Traduce el resultado del servicio a un código HTTP
    private IActionResult Respuesta(ResultadoAceptar resultado)
    {
        switch (resultado)
        {
            case ResultadoAceptar.Exito:
                return StatusCode(StatusCodes.Status201Created);
            case ResultadoAceptar.YaTomado:
                return Conflict("Otro conductor tomó el envío antes.");
            case ResultadoAceptar.NoCumple:
                return UnprocessableEntity("El envío ya no cumple las condiciones del viaje (fecha, lugar o desvío).");
            default:
                return NotFound();
        }
    }
}
