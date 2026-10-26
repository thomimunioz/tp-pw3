using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Entidades;
using RuteaYa.Logica.Viajes;

namespace RuteaYa.Web.Controllers;

public class ViajesController : Controller
{
    private readonly IViajeServicios _viajeServicios;
    private readonly UserManager<Usuario> _userManager;

    public ViajesController(IViajeServicios viajeServicios, UserManager<Usuario> userManager)
    {
        _viajeServicios = viajeServicios;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var usuario = await _userManager.GetUserAsync(User);

        if (usuario == null)
        {
            return Challenge();
        }

        var viajes = await _viajeServicios.ObtenerPorUsuarioAsync(usuario.Id);

        return View(viajes);
    }
}
