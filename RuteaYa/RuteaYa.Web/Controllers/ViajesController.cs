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
    public async Task<IActionResult> Detalle(int id)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return Challenge();
        }

        // El servicio filtra por Id del viaje y por UsuarioId: solo el propietario puede verlo.
        var viaje = await _viajeServicios.ObtenerDetallePorUsuarioAsync(id, usuario.Id);
        if (viaje == null)
        {
            return NotFound();
        }

        var modelo = new ViajeDetalleViewModel
        {
            Id = viaje.Id,
            OrigenDir = viaje.OrigenDir,
            DestinoDir = viaje.DestinoDir,
            HoraSalida = viaje.HoraSalida.ToString("HH:mm"),
            HoraLlegada = viaje.HoraLlegada.ToString("HH:mm"),
            MaxPaquetes = viaje.MaxPaquetes,
            PesoMaxKg = viaje.PesoMaxKg,
            DesvioMaxKm = viaje.DesvioMaxKm,
            DistanciaKm = viaje.DistanciaKm,
            DuracionMin = viaje.DuracionMin,
            Activo = viaje.Activo,
            Dias = viaje.Dias
                .Select(d => NombreDia(d.DiaSemana))
                .ToList(),
            Paradas = viaje.Paradas
                .OrderBy(p => p.Orden)
                .ToList(),
            PaquetesAsignados = viaje.Asignaciones
                .OrderBy(a => a.FechaViaje)
                .Select(a => new PaqueteAsignadoViewModel
                {
                    Id = a.Envio.Id,
                    OrigenDir = a.Envio.OrigenDir,
                    DestinoDir = a.Envio.DestinoDir,
                    PesoKg = a.Envio.PesoKg,
                    Tamano = a.Envio.Tamano.ToString(),
                    Estado = a.Envio.Estado.ToString(),
                    FechaViaje = a.FechaViaje
                })
                .ToList()
        };

        return View(modelo);
    }

    private static string NombreDia(DayOfWeek dia) => dia switch
    {
        DayOfWeek.Monday => "Lunes",
        DayOfWeek.Tuesday => "Martes",
        DayOfWeek.Wednesday => "Miércoles",
        DayOfWeek.Thursday => "Jueves",
        DayOfWeek.Friday => "Viernes",
        DayOfWeek.Saturday => "Sábado",
        DayOfWeek.Sunday => "Domingo",
        _ => dia.ToString()
    };

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



