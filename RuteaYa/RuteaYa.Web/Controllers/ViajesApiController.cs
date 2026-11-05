using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Entidades;
using RuteaYa.Logica.Servicios;

namespace RuteaYa.Web.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/viajes")]
public class ViajesApiController : ControllerBase
{
    private readonly IViajeServicios _viajeServicios;
    private readonly UserManager<Usuario> _userManager;

    public ViajesApiController(
        IViajeServicios viajeServicios,
        UserManager<Usuario> userManager)
    {
        _viajeServicios = viajeServicios;
        _userManager = userManager;
    }

    
    [HttpGet("{id:int}/recorrido")]
    public async Task<IActionResult> Recorrido(int id)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return Unauthorized();
        }

    
        var recorrido = await _viajeServicios.ObtenerRecorridoPorUsuarioAsync(id, usuario.Id);
        if (recorrido == null)
        {
            return NotFound();
        }

        return Ok(recorrido);
    }
}
