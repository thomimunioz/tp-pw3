using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RuteaYa.Logica.Oportunidades;
using RuteaYa.Logica.Oportunidades.Dtos;

namespace RuteaYa.Api.Controllers;

[ApiController]
[Route("api/viajes/{viajeId:int}/oportunidades")]
public class OportunidadesController : ControllerBase
{
    private readonly IOportunidadesServicios _oportunidades;

    public OportunidadesController(IOportunidadesServicios oportunidades) => _oportunidades = oportunidades;

    // GET api/viajes/5/oportunidades?usuarioId=2
    [HttpGet]
    [ProducesResponseType(typeof(List<OportunidadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<OportunidadDto>>> Buscar(int viajeId, [FromQuery, BindRequired] int usuarioId)
    {
        var oportunidades = await _oportunidades.BuscarAsync(usuarioId, viajeId);
        if (oportunidades == null)
            return NotFound();

        return Ok(oportunidades);
    }

    // GET api/viajes/5/oportunidades/8?usuarioId=2
    [HttpGet("{envioId:int}")]
    [ProducesResponseType(typeof(OportunidadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OportunidadDto>> Detalle(int viajeId, int envioId, [FromQuery, BindRequired] int usuarioId)
    {
        var detalle = await _oportunidades.ObtenerDetalleAsync(usuarioId, viajeId, envioId);
        if (detalle == null)
            return NotFound();

        return Ok(detalle);
    }
}
