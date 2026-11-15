using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Entidades.Enums;
using RuteaYa.Logica.Mapas;

namespace RuteaYa.Logica.Optimizacion;

public class ProyeccionMensualServicios : IProyeccionMensualServicios
{
    private readonly RuteaYaContext _context;

    public ProyeccionMensualServicios(RuteaYaContext context) => _context = context;

    public Task<ProyeccionMensualDto> CalcularAsync(
        int usuarioId,
        DateOnly mes,
        CriterioOptimizacion criterio)
    {
        return CalcularAsync(usuarioId, mes, criterio, DateOnly.FromDateTime(DateTime.Today));
    }

    public async Task<ProyeccionMensualDto> CalcularAsync(
        int usuarioId,
        DateOnly mes,
        CriterioOptimizacion criterio,
        DateOnly hoy)
    {
        var primerDia = new DateOnly(mes.Year, mes.Month, 1);
        var ultimoDia = new DateOnly(
            primerDia.Year,
            primerDia.Month,
            DateTime.DaysInMonth(primerDia.Year, primerDia.Month));
        var desde = primerDia > hoy ? primerDia : hoy;

        var proyeccion = new ProyeccionMensualDto
        {
            Mes = primerDia,
            Criterio = criterio
        };

        if (desde > ultimoDia)
        {
            CrearSemanas(proyeccion, primerDia, ultimoDia);
            return proyeccion;
        }

        var viajes = await _context.Viajes
            .AsNoTracking()
            .Include(v => v.Dias)
            .Include(v => v.Paradas)
            .Include(v => v.Asignaciones)
                .ThenInclude(a => a.Envio)
            .Where(v => v.UsuarioId == usuarioId && v.Activo)
            .ToListAsync();

        var enviosDisponibles = await _context.Envios
            .AsNoTracking()
            .Where(e => e.UsuarioId != usuarioId
                && e.Estado == EstadoEnvio.BuscandoConductor
                && e.FechaLimite >= desde
                && !_context.Asignaciones.Any(a => a.EnvioId == e.Id))
            .ToListAsync();

        var recorrencias = (from viaje in viajes
                            from fecha in FechasDelMes(desde, ultimoDia)
                            where viaje.Dias.Any(d => d.DiaSemana == fecha.DayOfWeek)
                            orderby fecha, viaje.Id
                            select new { Viaje = viaje, Fecha = fecha })
            .ToList();

        proyeccion.ViajesAnalizados = recorrencias.Count;

        var enviosPosibles = new HashSet<int>();
        var enviosUtilizados = new HashSet<int>();

        foreach (var recurrencia in recorrencias)
        {
            var viaje = recurrencia.Viaje;
            var asignados = viaje.Asignaciones
                .Where(a => a.FechaViaje == recurrencia.Fecha)
                .ToList();
            var paquetesLibres = Math.Max(0, viaje.MaxPaquetes - asignados.Count);
            var pesoLibre = Math.Max(0m, viaje.PesoMaxKg - asignados.Sum(a => a.Envio.PesoKg));

            var candidatos = new List<CandidatoEnvio>();
            foreach (var envio in enviosDisponibles)
            {
                if (envio.FechaLimite < recurrencia.Fecha
                    || envio.PesoKg > pesoLibre
                    || paquetesLibres == 0)
                {
                    continue;
                }

                var desvio = CalcularDesvio(viaje, envio);
                if (desvio > viaje.DesvioMaxKm)
                {
                    continue;
                }

                enviosPosibles.Add(envio.Id);
                if (enviosUtilizados.Contains(envio.Id))
                {
                    continue;
                }

                candidatos.Add(new CandidatoEnvio
                {
                    EnvioId = envio.Id,
                    Pago = envio.Monto,
                    PesoKg = envio.PesoKg,
                    DesvioKm = desvio,
                    Ruta = $"{envio.OrigenDir} → {envio.DestinoDir}"
                });
            }

            var resultado = Optimizador.Optimizar(
                candidatos,
                paquetesLibres,
                pesoLibre,
                viaje.DesvioMaxKm,
                criterio);

            var resumen = new ProyeccionViajeDto
            {
                ViajeId = viaje.Id,
                Fecha = recurrencia.Fecha,
                Origen = viaje.OrigenDir,
                Destino = viaje.DestinoDir,
                GananciaPotencial = resultado.GananciaTotal,
                DesvioTotalKm = resultado.DesvioTotalKm,
                Envios = resultado.Seleccionados.Select(c =>
                {
                    enviosUtilizados.Add(c.EnvioId);
                    var envio = enviosDisponibles.First(e => e.Id == c.EnvioId);
                    return new ProyeccionEnvioDto
                    {
                        EnvioId = envio.Id,
                        Descripcion = envio.Descripcion,
                        Ruta = c.Ruta,
                        Ganancia = c.Pago,
                        PesoKg = c.PesoKg,
                        DesvioKm = c.DesvioKm
                    };
                }).ToList()
            };

            proyeccion.Viajes.Add(resumen);
        }

        proyeccion.PaquetesPosibles = enviosPosibles.Count;
        proyeccion.GananciaPotencial = proyeccion.Viajes.Sum(v => v.GananciaPotencial);
        proyeccion.PromedioPorViaje = proyeccion.ViajesAnalizados == 0
            ? 0m
            : Math.Round(proyeccion.GananciaPotencial / proyeccion.ViajesAnalizados, 2);
        proyeccion.MejorRecorrido = OrdenarRecorridos(
                proyeccion.Viajes.Where(v => v.Envios.Count > 0),
                criterio)
            .ThenBy(v => v.Fecha)
            .FirstOrDefault();

        CrearSemanas(proyeccion, primerDia, ultimoDia);
        return proyeccion;
    }

    private static IEnumerable<DateOnly> FechasDelMes(DateOnly desde, DateOnly hasta)
    {
        for (var fecha = desde; ; fecha = fecha.AddDays(1))
        {
            yield return fecha;
            if (fecha == hasta)
            {
                yield break;
            }
        }
    }

    private static decimal CalcularDesvio(Viaje viaje, Envio envio)
    {
        var ruta = new List<Coordenada>
        {
            new() { Lat = viaje.OrigenLat, Lng = viaje.OrigenLng }
        };
        ruta.AddRange(viaje.Paradas
            .OrderBy(p => p.Orden)
            .Select(p => new Coordenada { Lat = p.Lat, Lng = p.Lng }));
        ruta.Add(new Coordenada { Lat = viaje.DestinoLat, Lng = viaje.DestinoLng });
        var distanciaHabitual = CalculadoraDistancias.DistanciaRutaKm(ruta);

        ruta.Insert(ruta.Count - 1, new Coordenada { Lat = envio.OrigenLat, Lng = envio.OrigenLng });
        ruta.Insert(ruta.Count - 1, new Coordenada { Lat = envio.DestinoLat, Lng = envio.DestinoLng });
        var distanciaConEnvio = CalculadoraDistancias.DistanciaRutaKm(ruta);

        return Math.Max(0m, distanciaConEnvio - distanciaHabitual);
    }

    private static IOrderedEnumerable<ProyeccionViajeDto> OrdenarRecorridos(
        IEnumerable<ProyeccionViajeDto> viajes,
        CriterioOptimizacion criterio)
    {
        return criterio switch
        {
            CriterioOptimizacion.MinimizarDesvios => viajes
                .OrderBy(v => v.DesvioTotalKm)
                .ThenByDescending(v => v.GananciaPotencial),
            CriterioOptimizacion.Equilibrio => viajes
                .OrderByDescending(v => v.GananciaPotencial / (1m + v.DesvioTotalKm))
                .ThenByDescending(v => v.GananciaPotencial)
                .ThenBy(v => v.DesvioTotalKm),
            _ => viajes
                .OrderByDescending(v => v.GananciaPotencial)
                .ThenBy(v => v.DesvioTotalKm)
        };
    }

    private static void CrearSemanas(
        ProyeccionMensualDto proyeccion,
        DateOnly primerDia,
        DateOnly ultimoDia)
    {
        var inicioSemana = primerDia.AddDays(-(((int)primerDia.DayOfWeek + 6) % 7));
        var semana = inicioSemana;
        while (semana <= ultimoDia)
        {
            var desde = semana < primerDia ? primerDia : semana;
            var diasRestantes = ultimoDia.DayNumber - semana.DayNumber;
            var hasta = semana.AddDays(Math.Min(6, diasRestantes));

            proyeccion.Semanas.Add(new ProyeccionSemanaDto
            {
                Desde = desde,
                Hasta = hasta,
                GananciaPotencial = proyeccion.Viajes
                    .Where(v => v.Fecha >= desde && v.Fecha <= hasta)
                    .Sum(v => v.GananciaPotencial)
            });

            if (hasta == ultimoDia)
            {
                break;
            }
            semana = semana.AddDays(7);
        }
    }
}
