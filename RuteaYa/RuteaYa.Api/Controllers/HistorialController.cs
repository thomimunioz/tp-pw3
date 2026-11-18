using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RuteaYa.Entidades.Enums;
using RuteaYa.Logica.Historial;
using RuteaYa.Logica.Historial.Dtos;

namespace RuteaYa.Api.Controllers;

[ApiController]
[Route("api/historial")]
public class HistorialController : ControllerBase
    {
    private readonly IHistorialServicios _historialServicios;

    public HistorialController(IHistorialServicios historialServicios)
        {
        _historialServicios = historialServicios;
        }

    // GET api/historial?usuarioId=2&mes=11&anio=2026&estado=5
    [HttpGet]
    [ProducesResponseType(typeof(HistorialResumenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HistorialResumenDto>> Obtener(
        [FromQuery, BindRequired] int usuarioId,
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

        var resumen = await _historialServicios.ObtenerHistorialAsync(usuarioId, filtro);
        return Ok(resumen);
        }
    }