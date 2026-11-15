using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RuteaYa.Logica.Optimizacion;
using RuteaYa.Logica.Optimizacion.Dtos;

namespace RuteaYa.Api.Controllers;

[ApiController]
[Route("api/viajes/{viajeId:int}/optimizacion")]
public class OptimizacionController : ControllerBase
{
    private readonly IOptimizadorServicios _optimizador;

    public OptimizacionController(IOptimizadorServicios optimizador) => _optimizador = optimizador;

    // GET api/viajes/5/optimizacion?usuarioId=2
    [HttpGet]
    [ProducesResponseType(typeof(OptimizacionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OptimizacionDto>> Calcular(int viajeId, [FromQuery, BindRequired] int usuarioId)
    {
        var optimizacion = await _optimizador.CalcularAsync(usuarioId, viajeId);
        if (optimizacion == null)
            return NotFound();

        return Ok(optimizacion);
    }
}
