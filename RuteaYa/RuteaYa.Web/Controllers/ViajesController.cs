using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Entidades;
using RuteaYa.Logica.Viajes;
using RuteaYa.Web.ViewModels;

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

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ViajeCrearViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ViajeCrearViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return Challenge();
        }

        var viaje = new Viaje
        {
            UsuarioId = usuario.Id,
            OrigenDir = modelo.OrigenDir.Trim(),
            DestinoDir = modelo.DestinoDir.Trim(),
            HoraSalida = modelo.HoraSalida,
            HoraLlegada = modelo.HoraLlegada,
            MaxPaquetes = modelo.MaxPaquetes,
            PesoMaxKg = modelo.PesoMaxKg,
            DesvioMaxKm = modelo.DesvioMaxKm,
            Activo = true
        };

        await _viajeServicios.CrearViajeAsync(viaje, modelo.DiasSeleccionados, modelo.ParadasIntermedias);

        return RedirectToAction(nameof(Index));
    }
}



