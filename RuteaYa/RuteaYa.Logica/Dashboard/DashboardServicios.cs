using RuteaYa.Entidades;
using RuteaYa.Logica.Account;
using RuteaYa.Logica.Envios;
using RuteaYa.Logica.Oportunidades;
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

        public DashboardServicios(
            IViajeServicios viajes,
            IEnvioServicios envios,
            IOportunidadesServicios oportunidades)
        {
            _viajes = viajes;
            _envios = envios;
            _oportunidades = oportunidades;
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

            foreach (var v in viajesActivos)
            {
                var ops = await _oportunidades.BuscarAsync(usuarioId, v.Id);
                dto.Oportunidades += ops?.Count ?? 0;
            }

            dto.ProximosViajes = proximos.Count;
            dto.EnviosPendientes = envios.Count;
            dto.PotencialMes = 0; // provisorio hasta la Pantalla 14: Potencial

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
