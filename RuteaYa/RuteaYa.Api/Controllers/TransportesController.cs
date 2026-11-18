using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Api.Auth;
using RuteaYa.Logica.Transportes;
using RuteaYa.Logica.Transportes.Dtos;

namespace RuteaYa.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/transportes")]
public class TransportesController : ControllerBase
{
    private readonly ITransporteServicios _transporteServicios;

    public TransportesController(ITransporteServicios transporteServicios)
    {
        _transporteServicios = transporteServicios;
    }

    // GET api/transportes
    [HttpGet]
    [ProducesResponseType(typeof(List<TransporteListadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<TransporteListadoDto>>> Listar()
    {
        var usuarioId = User.ObtenerUsuarioId();
        var transportes = await _transporteServicios.ListarMisTransportesAsync(usuarioId);
        return Ok(transportes);
    }

    // PUT api/transportes/5/retirado
    [HttpPut("{envioId:int}/retirado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarcarRetirado(int envioId)
    {
        var usuarioId = User.ObtenerUsuarioId();
        var resultado = await _transporteServicios.MarcarRetiradoAsync(usuarioId, envioId);
        return MapearRespuesta(resultado);
    }

    // PUT api/transportes/5/en-camino
    [HttpPut("{envioId:int}/en-camino")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarcarEnCamino(int envioId)
    {
        var usuarioId = User.ObtenerUsuarioId();
        var resultado = await _transporteServicios.MarcarEnCaminoAsync(usuarioId, envioId);
        return MapearRespuesta(resultado);
    }

    // PUT api/transportes/5/entregado
    [HttpPut("{envioId:int}/entregado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarcarEntregado(int envioId)
    {
        var usuarioId = User.ObtenerUsuarioId();
        var resultado = await _transporteServicios.MarcarEntregadoAsync(usuarioId, envioId);
        return MapearRespuesta(resultado);
    }

    private IActionResult MapearRespuesta(ResultadoActualizarTransporte resultado)
    {
        if (resultado.NoEncontrado)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: resultado.Error);
        }

        if (!resultado.ResultadoExitoso)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: resultado.Error);
        }

        return Ok(new
        {
            mensaje = "Estado actualizado correctamente."
        });
    }
}