using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Api.Autenticacion;
using RuteaYa.Logica.Historial;
using RuteaYa.Logica.Historial.Dtos;

namespace RuteaYa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/historial")]
public class HistorialController : ControllerBase
{
    private readonly IHistorialServicios _historialServicios;

    public HistorialController(IHistorialServicios historialServicios)
    {
        _historialServicios = historialServicios;
    }

    // GET api/historial?mes=11&anio=2026&estado=5
    [HttpGet]
    [ProducesResponseType(typeof(HistorialResumenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HistorialResumenDto>> Obtener(
        [FromQuery] int? mes,
        [FromQuery] int? anio,
        [FromQuery] int? estado)
    {
        EstadoEnvio? estadoEnum = estado.HasValue ? (EstadoEnvio)estado.Value : null;

        var filtro = new FiltroHistorialDto
        {
            Mes = mes,
            Anio = anio,
            Estado = estadoEnum
        };

        var usuarioId = User.ObtenerUsuarioId();

        var resumen = await _historialServicios.ObtenerHistorialAsync(usuarioId, filtro);
        return Ok(resumen);
    }
}