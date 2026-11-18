using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Api.Auth;
using RuteaYa.Logica.Optimizacion;
using RuteaYa.Logica.Optimizacion.Dtos;

namespace RuteaYa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/viajes/{viajeId:int}/optimizacion")]
public class OptimizacionController : ControllerBase
{
    private readonly IOptimizadorServicios _optimizador;

    public OptimizacionController(IOptimizadorServicios optimizador) => _optimizador = optimizador;

    // GET api/viajes/5/optimizacion
    [HttpGet]
    [ProducesResponseType(typeof(OptimizacionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OptimizacionDto>> Calcular(int viajeId)
    {
        var usuarioId = User.ObtenerUsuarioId();
        var optimizacion = await _optimizador.CalcularAsync(usuarioId, viajeId);
        if (optimizacion == null)
            return NotFound();

        return Ok(optimizacion);
    }
}
