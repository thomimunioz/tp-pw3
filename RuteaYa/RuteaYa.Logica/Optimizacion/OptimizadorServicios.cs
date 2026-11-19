using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades.Data;
using RuteaYa.Logica.Oportunidades;
using RuteaYa.Logica.Optimizacion.Dtos;

namespace RuteaYa.Logica.Optimizacion;

public class OptimizadorServicios : IOptimizadorServicios
{
    private readonly RuteaYaContext _context;
    private readonly IOportunidadesServicios _oportunidades;

    public OptimizadorServicios(RuteaYaContext context, IOportunidadesServicios oportunidades)
    {
        _context = context;
        _oportunidades = oportunidades;
    }

    public Task<OptimizacionDto?> CalcularAsync(int usuarioId, int viajeId)
    {
        return CalcularAsync(usuarioId, viajeId, DateOnly.FromDateTime(DateTime.Today));
    }

    public async Task<OptimizacionDto?> CalcularAsync(int usuarioId, int viajeId, DateOnly hoy)
    {
        var viaje = await _context.Viajes
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == viajeId && v.UsuarioId == usuarioId && v.Activo);
        if (viaje == null)
        {
            return null;
        }

        var optimizacion = new OptimizacionDto
        {
            ViajeId = viaje.Id,
            Origen = viaje.OrigenDir,
            Destino = viaje.DestinoDir,
            MaxPaquetes = viaje.MaxPaquetes,
            PesoMaxKg = viaje.PesoMaxKg,
            DesvioMaxKm = viaje.DesvioMaxKm,
            PaquetesDisponibles = viaje.MaxPaquetes,
            PesoDisponibleKg = viaje.PesoMaxKg,
            DesvioDisponibleKm = viaje.DesvioMaxKm
        };

        // Las oportunidades ya vienen filtradas por fecha límite, lugar libre y desvío máximo
        var oportunidades = await _oportunidades.BuscarAsync(usuarioId, viajeId, hoy);
        if (oportunidades == null || oportunidades.Count == 0)
        {
            return optimizacion;
        }

        // Todos los envíos de una combinación viajan el mismo día,
        // así que se combinan solo los de la próxima fecha con lugar
        var fecha = oportunidades.Min(o => o.FechaViaje);
        var delDia = oportunidades.Where(o => o.FechaViaje == fecha).ToList();

        optimizacion.FechaViaje = fecha;
        optimizacion.PaquetesDisponibles = delDia[0].PaquetesDisponibles;
        optimizacion.PesoDisponibleKg = delDia[0].PesoDisponibleKg;
        optimizacion.DesvioDisponibleKm = delDia[0].DesvioDisponibleKm;
        optimizacion.Candidatos = delDia
            .Select(o => new CandidatoEnvio
            {
                EnvioId = o.EnvioId,
                Pago = o.Pago,
                PesoKg = o.PesoKg,
                DesvioKm = o.DesvioKm,
                Ruta = $"{o.OrigenDireccion} → {o.DestinoDireccion}",
                Descripcion = o.Descripcion,
                Tamano = o.Tamano
            })
            .ToList();

        optimizacion.Resultado = Optimizador.Optimizar(
            optimizacion.Candidatos,
            optimizacion.PaquetesDisponibles,
            optimizacion.PesoDisponibleKg,
            optimizacion.DesvioDisponibleKm);

        return optimizacion;
    }
}
