using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Api.Autenticacion;
using RuteaYa.Logica.Envios;
using RuteaYa.Logica.Envios.Dtos;

namespace RuteaYa.Api.Controllers;

[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[Route("api/envios")]
public class EnviosController : ControllerBase
{
    private readonly IEnvioServicios _envios;

    public EnviosController(IEnvioServicios envios)
    {
        _envios = envios;
    }

    // GET api/envios?estado=1
    [HttpGet]
    [ProducesResponseType(typeof(List<EnvioListadoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<EnvioListadoDto>>> Listar([FromQuery] EstadoEnvio? estado)
    {
        var usuarioId = User.ObtenerUsuarioId();

        if (estado.HasValue && !Enum.IsDefined(estado.Value))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "El estado indicado no existe.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var envios = await _envios.ListarAsync(usuarioId, estado);
        return Ok(envios);
    }

    // GET api/envios/5
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EnvioDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnvioDetalleDto>> Detalle(int id)
    {
        var usuarioId = User.ObtenerUsuarioId();

        var detalle = await _envios.ObtenerDetalleAsync(usuarioId, id);
        if (detalle == null)
        {
            return NotFound();
        }

        return Ok(detalle);
    }

    // POST api/envios
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearEnvioDto dto)
    {
        var usuarioId = User.ObtenerUsuarioId();

        var resultado = await _envios.CrearAsync(usuarioId, dto);
        if (!resultado.ResultadoExitoso)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["errores"] = resultado.Errores.ToArray()
            }));
        }

        return CreatedAtAction(nameof(Detalle), new { id = resultado.EnvioId }, new { id = resultado.EnvioId });
    }

    // PUT api/envios/5/cancelar
    [HttpPut("{id:int}/cancelar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancelar(int id)
    {
        var usuarioId = User.ObtenerUsuarioId();

        var resultado = await _envios.CancelarAsync(usuarioId, id);
        if (resultado.NoEncontrado)
        {
            return NotFound();
        }

        if (!resultado.ResultadoExitoso)
        {
            return Conflict(new ProblemDetails
            {
                Title = "No se pudo cancelar el envío.",
                Detail = resultado.Error,
                Status = StatusCodes.Status409Conflict
            });
        }

        return NoContent();
    }
}