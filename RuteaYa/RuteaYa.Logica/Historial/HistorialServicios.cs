using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades.Data;
using RuteaYa.Logica.Historial.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuteaYa.Logica.Historial
    {

    public class HistorialServicios : IHistorialServicios
        {

        private readonly RuteaYaContext _context;

        public HistorialServicios(RuteaYaContext context)
            {
            _context = context;
            }

        public async Task<HistorialResumenDto> ObtenerHistorialAsync(int usuarioId, FiltroHistorialDto? filtro = null)
            {

            // Solo las operaciones terminadas del conductor: entregadas o canceladas.
            var queryAsignaciones = _context.Asignaciones
            .AsNoTracking()
            .Include(a => a.Viaje)
            .Include(a => a.Envio)
            .ThenInclude(e => e.Historial)
            .Where(a => a.Viaje.UsuarioId == usuarioId)
            .Where(a => a.Envio.Estado == EstadoEnvio.Entregado || a.Envio.Estado == EstadoEnvio.Cancelado);

            if (filtro?.Mes.HasValue == true && filtro.Mes.Value >= 1 && filtro.Mes.Value <= 12)
                {
                    var mes = filtro.Mes.Value;
                    queryAsignaciones = queryAsignaciones.Where(a => a.FechaViaje.Month == mes);
                }

            if (filtro?.Anio.HasValue == true && filtro.Anio.Value > 2000)
                {
                    var anio = filtro.Anio.Value;
                    queryAsignaciones = queryAsignaciones.Where(a => a.FechaViaje.Year == anio);
                }

            if (filtro?.Estado.HasValue == true)
                {
                    var estado = filtro.Estado.Value;
                    queryAsignaciones = queryAsignaciones.Where(a => a.Envio.Estado == estado);
                }

            // Las tarjetas de arriba se calculan sobre la misma lista que se muestra abajo
            var entregadosQuery = queryAsignaciones.Where(a => a.Envio.Estado == EstadoEnvio.Entregado);
            var totalEnviosEntregados = await entregadosQuery.CountAsync();
            var gananciaTotal = await entregadosQuery.SumAsync(a => a.Ganancia);
            // Cada salida cuenta como un viaje: el mismo recorrido hecho dos días distintos son 2 viajes
            var viajesDistintos = await entregadosQuery.Select(a => new { a.ViajeId, a.FechaViaje }).Distinct().CountAsync();
            var totalCancelados = await queryAsignaciones.CountAsync(a => a.Envio.Estado == EstadoEnvio.Cancelado);

            var operacionesDb = await queryAsignaciones
                .OrderByDescending(a => a.FechaViaje)
                .ThenByDescending(a => a.AsignadoEn)
                .ToListAsync();

            var operacionesDto = operacionesDb.Select(a => new ItemHistorialDto
                {
                    AsignacionId = a.Id,
                    EnvioId = a.EnvioId,
                    ViajeId = a.ViajeId,
                    Descripcion = a.Envio.Descripcion,
                    Recorrido = $"{a.Envio.OrigenDir} → {a.Envio.DestinoDir}",
                    ViajeRecorrido = $"{a.Viaje.OrigenDir} → {a.Viaje.DestinoDir}",
                    Detalle = $"{a.FechaViaje:dd/MM/yyyy} • {a.Envio.Descripcion} • Viaje {a.Viaje.OrigenDir} → {a.Viaje.DestinoDir}",
                    Fecha = a.FechaViaje,
                    // Cuándo se entregó o se canceló (sale del historial de estados del envío)
                    FechaCierre = a.Envio.Historial
                        .Where(h => h.EstadoNuevo == a.Envio.Estado)
                        .Select(h => (DateTime?)h.Fecha)
                        .Max(),
                    MontoGanancia = a.Ganancia,
                    Estado = a.Envio.Estado
                    }).ToList();

            return new HistorialResumenDto
                {
                ViajesRealizados = viajesDistintos,
                EnviosTransportados = totalEnviosEntregados,
                EnviosCancelados = totalCancelados,
                GananciaEfectiva = gananciaTotal,
                Operaciones = operacionesDto
                };
            }

        private static string ObtenerNombreMes(int mes)
            {
            return mes switch
                {
                    1 => "Enero",
                    2 => "Febrero",
                    3 => "Marzo",
                    4 => "Abril",
                    5 => "Mayo",
                    6 => "Junio",
                    7 => "Julio",
                    8 => "Agosto",
                    9 => "Septiembre",
                    10 => "Octubre",
                    11 => "Noviembre",
                    12 => "Diciembre",
                    _ => string.Empty
                    };
            }
        }
    }