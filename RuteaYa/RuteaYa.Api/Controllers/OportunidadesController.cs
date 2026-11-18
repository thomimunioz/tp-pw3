using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Api.Autenticacion;
using RuteaYa.Logica.Oportunidades;
using RuteaYa.Logica.Oportunidades.Dtos;

namespace RuteaYa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/viajes/{viajeId:int}/oportunidades")]
public class OportunidadesController : ControllerBase
{
    private readonly IOportunidadesServicios _oportunidades;

    public OportunidadesController(IOportunidadesServicios oportunidades) => _oportunidades = oportunidades;

    // GET api/viajes/5/oportunidades
    [HttpGet]
    [ProducesResponseType(typeof(List<OportunidadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<OportunidadDto>>> Buscar(int viajeId)
    {
        var usuarioId = User.ObtenerUsuarioId();
        var oportunidades = await _oportunidades.BuscarAsync(usuarioId, viajeId);
        if (oportunidades == null)
            return NotFound();

        return Ok(oportunidades);
    }

    // GET api/viajes/5/oportunidades/8
    [HttpGet("{envioId:int}")]
    [ProducesResponseType(typeof(OportunidadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OportunidadDto>> Detalle(int viajeId, int envioId)
    {
        var usuarioId = User.ObtenerUsuarioId();
        var detalle = await _oportunidades.ObtenerDetalleAsync(usuarioId, viajeId, envioId);
        if (detalle == null)
            return NotFound();

        return Ok(detalle);
    }
}
