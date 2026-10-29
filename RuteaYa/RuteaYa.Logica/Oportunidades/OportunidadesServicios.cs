using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Logica.Mapas;
using RuteaYa.Logica.Oportunidades.Dtos;

using Microsoft.EntityFrameworkCore;

namespace RuteaYa.Logica.Oportunidades;

public class OportunidadesServicios : IOportunidadesServicios
{
    private readonly RuteaYaContext _context;

    public OportunidadesServicios(RuteaYaContext context)
    {
        _context = context;
    }

    public Task<List<OportunidadDto>?> BuscarAsync(int usuarioId, int viajeId)
    {
        return BuscarAsync(usuarioId, viajeId, DateOnly.FromDateTime(DateTime.Today));
    }

    public async Task<List<OportunidadDto>?> BuscarAsync(int usuarioId, int viajeId, DateOnly hoy)
    {
        var viaje = await CargarViajeAsync(usuarioId, viajeId);

        if (viaje == null)
        {
            return null;
        }

        var envios = await EnviosDisponibles(usuarioId, hoy).ToListAsync();

        decimal distanciaHabitual = CalculadoraDistancias.DistanciaRutaKm(ArmarRuta(viaje, null));
        var oportunidades = new List<OportunidadDto>();

        foreach (var envio in envios)
        {
            var oportunidad = CalcularOportunidad(viaje, envio, hoy, distanciaHabitual);
            if (oportunidad != null)
            {
                oportunidades.Add(oportunidad);
            }
        }
        return oportunidades;
    }

    public Task<OportunidadDto?> ObtenerDetalleAsync(int usuarioId, int viajeId, int envioId)
    {
        return ObtenerDetalleAsync(usuarioId, viajeId, envioId, DateOnly.FromDateTime(DateTime.Today));
    }

    public async Task<OportunidadDto?> ObtenerDetalleAsync(int usuarioId, int viajeId, int envioId, DateOnly hoy)
    {
        var viaje = await CargarViajeAsync(usuarioId, viajeId);
        if (viaje == null)
        {
            return null;
        }

        var envio = await EnviosDisponibles(usuarioId, hoy)
            .FirstOrDefaultAsync(e => e.Id == envioId);
        if (envio == null)
        {
            return null;
        }

        decimal distanciaHabitual = CalculadoraDistancias.DistanciaRutaKm(ArmarRuta(viaje, null));
        return CalcularOportunidad(viaje, envio, hoy, distanciaHabitual);
    }

    private async Task<Viaje?> CargarViajeAsync(int usuarioId, int viajeId)
    {
        return await _context.Viajes
            .Include(v => v.Paradas)
            .Include(v => v.Dias)
            .Include(v => v.Asignaciones).ThenInclude(a => a.Envio)
            .FirstOrDefaultAsync(v => v.Id == viajeId && v.UsuarioId == usuarioId && v.Activo);
    }

    private IQueryable<Envio> EnviosDisponibles(int usuarioId, DateOnly hoy)
    {
        return _context.Envios
            .Where(e => e.Estado == EstadoEnvio.BuscandoConductor
                && e.Asignacion == null
                && e.UsuarioId != usuarioId
                && e.FechaLimite >= hoy);
    }

    private static OportunidadDto? CalcularOportunidad(Viaje viaje, Envio envio, DateOnly hoy, decimal distanciaHabitual)
    {
        DateOnly? fechaViaje = BuscarFechaConLugar(viaje, envio, hoy);
        if (fechaViaje == null)
        {
            return null;
        }

        decimal distanciaConEnvio = CalculadoraDistancias.DistanciaRutaKm(ArmarRuta(viaje, envio));
        decimal desvio = distanciaConEnvio - distanciaHabitual;

        if (desvio > viaje.DesvioMaxKm)
        {
            return null;
        }

        var asignadosEseDia = AsignacionesDelDia(viaje, fechaViaje.Value);
        return new OportunidadDto
        {
            EnvioId = envio.Id,
            OrigenDireccion = envio.OrigenDir,
            DestinoDireccion = envio.DestinoDir,
            Descripcion = envio.Descripcion,
            Pago = envio.Monto,
            PesoKg = envio.PesoKg,
            FechaViaje = fechaViaje.Value,
            DistanciaHabitualKm = distanciaHabitual,
            DistanciaConEnvioKm = distanciaConEnvio,
            DesvioKm = desvio,
            TiempoHabitualMin = CalculadoraDistancias.MinutosDeViaje(distanciaHabitual),
            TiempoExtraMin = CalculadoraDistancias.MinutosDeViaje(desvio),
            GananciaPorKm = desvio > 0 ? Math.Round(envio.Monto / desvio, 2) : envio.Monto,
            PaquetesDisponibles = viaje.MaxPaquetes - asignadosEseDia.Count,
            PesoDisponibleKg = viaje.PesoMaxKg - asignadosEseDia.Sum(a => a.Envio.PesoKg)
        };
    }

    private static List<Coordenada> ArmarRuta(Viaje viaje, Envio? envio)
    {
        var ruta = new List<Coordenada>();

        ruta.Add(new Coordenada {Lat = viaje.OrigenLat, Lng = viaje.OrigenLng });

        foreach (var parada in viaje.Paradas.OrderBy(p => p.Orden))
        {
            ruta.Add(new Coordenada { Lat = parada.Lat, Lng = parada.Lng });
        }

        if (envio != null)
        {
            ruta.Add(new Coordenada { Lat = envio.OrigenLat, Lng = envio.OrigenLng });
            ruta.Add(new Coordenada { Lat = envio.DestinoLat, Lng = envio.DestinoLng });
        }

        ruta.Add(new Coordenada { Lat = viaje.DestinoLat, Lng = viaje.DestinoLng });

        return ruta;
    }

    private static List<Asignacion> AsignacionesDelDia(Viaje viaje, DateOnly fecha)
    {
        return viaje.Asignaciones.Where(a => a.FechaViaje == fecha).ToList();
    }

    private static DateOnly? BuscarFechaConLugar(Viaje viaje, Envio envio, DateOnly hoy)
    {
        for(var fecha = hoy; fecha <= envio.FechaLimite; fecha = fecha.AddDays(1))
        {
            bool viajaEseDia = viaje.Dias.Any(d => d.DiaSemana == fecha.DayOfWeek);

            if (!viajaEseDia)
            {
                continue;
            }

            var asignadosEseDia = AsignacionesDelDia(viaje, fecha);

            bool entraPorCantidad = asignadosEseDia.Count < viaje.MaxPaquetes;
            bool entraPorPeso = asignadosEseDia.Sum(a => a.Envio.PesoKg) + envio.PesoKg <= viaje.PesoMaxKg;

            if (entraPorCantidad && entraPorPeso)
            {
                return fecha;
            }

        }
        return null;   
    }

}