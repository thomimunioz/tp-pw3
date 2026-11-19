using RuteaYa.Entidades;
using RuteaYa.Logica.Account;
using RuteaYa.Logica.Envios;
using RuteaYa.Logica.Oportunidades;
using RuteaYa.Logica.Optimizacion;
using RuteaYa.Logica.Transportes;
using RuteaYa.Logica.Viajes;
using System;
using System.Collections.Generic;
using System.Text;

namespace RuteaYa.Logica.Dashboard
{
    public interface IDashboardServicios
    {
        Task<DashboardDto> ObtenerAsync(int usuarioId);
    }

    public class DashboardServicios : IDashboardServicios
    {
        private readonly IViajeServicios _viajes;
        private readonly IEnvioServicios _envios;
        private readonly IOportunidadesServicios _oportunidades;
        private readonly IProyeccionMensualServicios _proyeccionMensual;
        private readonly ITransporteServicios _transportes;

        public DashboardServicios(
            IViajeServicios viajes,
            IEnvioServicios envios,
            IOportunidadesServicios oportunidades,
            IProyeccionMensualServicios proyeccionMensual,
            ITransporteServicios transportes)
        {
            _viajes = viajes;
            _envios = envios;
            _oportunidades = oportunidades;
            _proyeccionMensual = proyeccionMensual;
            _transportes = transportes;
        }

        public async Task<DashboardDto> ObtenerAsync(int usuarioId)
        {
            var dto = new DashboardDto();
            var ahora = DateTime.Now;

            var viajesActivos = (await _viajes.ObtenerPorUsuarioAsync(usuarioId))
                .Where(v => v.Activo)
                .ToList();

            // Se piden con detalle porque el listado no trae los días
            var proximos = new List<Viaje>();
            foreach (var v in viajesActivos)
            {
                var detalle = await _viajes.ObtenerDetallePorUsuarioAsync(v.Id, usuarioId);
                if (detalle != null)
                    proximos.Add(detalle);
            }
            proximos = proximos.OrderBy(v => ProximaSalida(v, ahora)).ToList();

            dto.Viajes = proximos.Take(3).Select(v => new ViajeResumenDto(
                    v.OrigenDir,
                    v.DestinoDir,
                    NombresDias(v),
                    $"{v.HoraSalida.ToString("HH:mm")} — {v.HoraLlegada.ToString("HH:mm")}")).ToList();

            var envios = (await _envios.ListarAsync(usuarioId, null))
                .Where(e => e.Estado != EstadoEnvio.Entregado && e.Estado != EstadoEnvio.Cancelado)
                .ToList();

            dto.Envios = envios.Take(3).Select(e => new EnvioResumenDto(
                e.OrigenDir, e.DestinoDir, e.Descripcion, e.Monto, e.Estado)).ToList();

            // Un mismo envío puede servirle a dos viajes: se cuenta una sola vez
            var enviosConOportunidad = new HashSet<int>();
            foreach (var v in viajesActivos)
            {
                var ops = await _oportunidades.BuscarAsync(usuarioId, v.Id);
                if (ops != null)
                {
                    foreach (var o in ops)
                    {
                        enviosConOportunidad.Add(o.EnvioId);
                    }
                }
            }
            dto.Oportunidades = enviosConOportunidad.Count;

            dto.ProximosViajes = proximos.Count;
            dto.EnviosPendientes = envios.Count;

            if (envios.Count > 0 && envios.All(e => e.Estado == envios[0].Estado))
            {
                dto.EstadoEnviosPendientes = envios[0].Estado;
            }

            // Paquetes que el usuario lleva como conductor y todavía no entregó
            var transportes = await _transportes.ListarMisTransportesAsync(usuarioId);
            if (transportes != null)
            {
                dto.ParaEntregar = transportes.Count(t =>
                    t.Estado == EstadoEnvio.ConductorAsignado ||
                    t.Estado == EstadoEnvio.Retirado ||
                    t.Estado == EstadoEnvio.EnCamino);
            }

            var mesActual = new DateOnly(ahora.Year, ahora.Month, 1);
            var proyeccion = await _proyeccionMensual.CalcularAsync(
                usuarioId,
                mesActual,
                CriterioOptimizacion.MaximizarGanancias);
            dto.PotencialMes = proyeccion.GananciaPotencial;

            return dto;
        }

        // Fecha y hora de la próxima salida del viaje, contando desde ahora
        private static DateTime ProximaSalida(Viaje v, DateTime ahora)
        {
            for (int i = 0; i <= 7; i++)
            {
                var fecha = ahora.Date.AddDays(i);
                if (v.Dias.Any(d => d.DiaSemana == fecha.DayOfWeek))
                {
                    var salida = fecha.Add(v.HoraSalida.ToTimeSpan());
                    if (salida >= ahora) return salida;
                }
            }
            return DateTime.MaxValue; // viaje sin días cargados: va al final
        }

        private static string NombresDias(Viaje v)
        {
            string[] nombres = { "Dom", "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb" };
            return string.Join(", ", v.Dias.OrderBy(d => d.DiaSemana).Select(d => nombres[(int)d.DiaSemana]));
        }
    }
}
