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

            // Asignaciones del conductor.
            var queryAsignaciones = _context.Asignaciones
            .AsNoTracking()
            .Include(a => a.Viaje)
            .Include(a => a.Envio)
            .Where(a => a.Viaje.UsuarioId == usuarioId);

            var entregadosQuery = queryAsignaciones.Where(a => a.Envio.Estado == EstadoEnvio.Entregado);
            var totalEnviosEntregados = await entregadosQuery.CountAsync();
            var gananciaTotal = await entregadosQuery.SumAsync(a => a.Ganancia);
            var viajesDistintos = await entregadosQuery.Select(a => a.ViajeId).Distinct().CountAsync();

            var queryFiltrada = queryAsignaciones;

            if (filtro?.Estado.HasValue == true)
                {
                    queryFiltrada = queryFiltrada.Where(a => a.Envio.Estado == filtro.Estado.Value);
                }
            else
                {

                    queryFiltrada = queryFiltrada.Where(a => a.Envio.Estado == EstadoEnvio.Entregado || a.Envio.Estado == EstadoEnvio.Cancelado);
                }

            if (filtro?.Mes.HasValue == true && filtro.Mes.Value >= 1 && filtro.Mes.Value <= 12)
                {
                    var mes = filtro.Mes.Value;
                    queryFiltrada = queryFiltrada.Where(a => a.FechaViaje.Month == mes);
                }

            if (filtro?.Anio.HasValue == true && filtro.Anio.Value > 2000)
                {
                    var anio = filtro.Anio.Value;
                    queryFiltrada = queryFiltrada.Where(a => a.FechaViaje.Year == anio);
                }

            var operacionesDb = await queryFiltrada
                .OrderByDescending(a => a.FechaViaje)
                .ThenByDescending(a => a.AsignadoEn)
                .ToListAsync();

            var operacionesDto = operacionesDb.Select(a => new ItemHistorialDto
                {
                    AsignacionId = a.Id,
                    EnvioId = a.EnvioId,
                    ViajeId = a.ViajeId,
                    Recorrido = $"{a.Viaje.OrigenDir} → {a.Viaje.DestinoDir}",
                    Detalle = $"{ObtenerNombreMes(a.FechaViaje.Month)} • {a.Envio.Descripcion}",
                    Fecha = a.FechaViaje,
                    MontoGanancia = a.Ganancia,
                    Estado = a.Envio.Estado
                    }).ToList();

            return new HistorialResumenDto
                {
                ViajesRealizados = viajesDistintos,
                EnviosTransportados = totalEnviosEntregados,
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