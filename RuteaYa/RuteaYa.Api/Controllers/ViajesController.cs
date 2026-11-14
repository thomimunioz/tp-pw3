using Microsoft.AspNetCore.Mvc;
using RuteaYa.Logica.Viajes;

namespace RuteaYa.Api.Controllers;

[ApiController]
[Route("api/viajes")]
public class ViajesController : ControllerBase
{
    private readonly IViajeListadoServicios _servicios;

    public ViajesController(IViajeListadoServicios servicios) => _servicios = servicios;

    [HttpGet]
    public async Task<ActionResult<List<ViajeListadoDto>>> Get()
        => Ok(await _servicios.ObtenerActivosAsync());
}