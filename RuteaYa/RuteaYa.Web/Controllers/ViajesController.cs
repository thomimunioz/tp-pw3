using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Entidades;
using RuteaYa.Logica.Viajes;
using RuteaYa.Web.Models;
using RuteaYa.Web.Models.Viajes;
using System.Security.Claims;

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

        // ACÁ ESTÁ LA SOLUCIÓN DEL ERROR AL CREAR
        try
        {
            await _viajeServicios.CrearViajeAsync(viaje, modelo.DiasSeleccionados, modelo.ParadasIntermedias);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            // Ataja el error del mapa y lo manda a la vista
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado al guardar.");
            return View(modelo);
        }
    }

    // --- ACÁ EMPIEZAN LOS MÉTODOS DE EDICIÓN ---

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null) return Challenge();

        // 1. Buscamos el viaje en la base de datos
        var viajeDb = await _viajeServicios.ObtenerPorIdAsync(id, usuario.Id);
        if (viajeDb == null) return NotFound();

        // 2. Traducimos la Entidad al ViewModel para mandarlo a la pantalla
        var modelo = new ViajeEditarViewModel
        {
            Id = viajeDb.Id,
            OrigenDir = viajeDb.OrigenDir,
            DestinoDir = viajeDb.DestinoDir,
            HoraSalida = viajeDb.HoraSalida,
            HoraLlegada = viajeDb.HoraLlegada,
            MaxPaquetes = viajeDb.MaxPaquetes,
            PesoMaxKg = viajeDb.PesoMaxKg,
            DesvioMaxKm = viajeDb.DesvioMaxKm,
            DiasSeleccionados = viajeDb.Dias.Select(d => (int)d.DiaSemana).ToList(),
            // Unimos las paradas con coma para que el usuario las edite fácil en la caja de texto
            ParadasIntermedias = viajeDb.Paradas.Any() ? string.Join(", ", viajeDb.Paradas.OrderBy(p => p.Orden).Select(p => p.Direccion)) : string.Empty
        };

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ViajeEditarViewModel modelo)
    {
        if (id != modelo.Id) return BadRequest();

        if (!ModelState.IsValid) return View(modelo);

        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null) return Challenge();

        try
        {
            // Empaquetamos solo los datos modificables en un objeto temporal
            var viajeActualizado = new Viaje
            {
                Id = modelo.Id,
                OrigenDir = modelo.OrigenDir.Trim(),
                DestinoDir = modelo.DestinoDir.Trim(),
                HoraSalida = modelo.HoraSalida,
                HoraLlegada = modelo.HoraLlegada,
                MaxPaquetes = modelo.MaxPaquetes,
                PesoMaxKg = modelo.PesoMaxKg,
                DesvioMaxKm = modelo.DesvioMaxKm
            };

            // Mandamos todo al servicio
            await _viajeServicios.EditarViajeAsync(viajeActualizado, modelo.DiasSeleccionados, modelo.ParadasIntermedias);

            // Si salió todo bien, lo devolvemos a la lista de "Mis viajes"
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            // Si las coordenadas fallan, pintamos el error en rojo en la pantalla
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar el viaje.");
            return View(modelo);
        }
    }
}