using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Logica.Oportunidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace RuteaYa.Logica.Asignaciones
{
    public interface IAsignacionesServicios
    {
        Task<ResultadoAceptar> AceptarAsync(int usuarioId, int viajeId, int envioId);
        Task<ResultadoAceptar> AceptarAsync(int usuarioId, int viajeId, int envioId, DateOnly hoy);
        Task<ResultadoAceptar> ConfirmarCombinacionAsync(int usuarioId, int viajeId, List<int> envioIds);
        Task<ResultadoAceptar> ConfirmarCombinacionAsync(int usuarioId, int viajeId, List<int> envioIds, DateOnly hoy);
    }
    public class AsignacionesServicios : IAsignacionesServicios
    {
        private readonly RuteaYaContext _context;
        private readonly IOportunidadesServicios _oportunidades;

        public AsignacionesServicios(RuteaYaContext context, IOportunidadesServicios oportunidades)
        {
            _context = context;
            _oportunidades = oportunidades;
        }

        public Task<ResultadoAceptar> AceptarAsync(int usuarioId, int viajeId, int envioId)
        {
            return AceptarAsync(usuarioId, viajeId, envioId, DateOnly.FromDateTime(DateTime.Today));
        }

        public async Task<ResultadoAceptar> AceptarAsync(int usuarioId, int viajeId, int envioId, DateOnly hoy)
        {
            bool viajeEsDelUsuario = await _context.Viajes
                .AnyAsync(v => v.Id == viajeId && v.UsuarioId == usuarioId && v.Activo);
            if (!viajeEsDelUsuario)
            {
                return ResultadoAceptar.NoEncontrado;
            }

            var envio = await _context.Envios
                .Include(e => e.Asignacion)
                .FirstOrDefaultAsync(e => e.Id == envioId);
            if (envio == null)
            {
                return ResultadoAceptar.NoEncontrado;
            }

            if (envio.Estado == EstadoEnvio.Cancelado)
            {
                return ResultadoAceptar.NoCumple;
            }

            if (envio.Estado == EstadoEnvio.ConductorAsignado || envio.Asignacion != null)
            {
                return ResultadoAceptar.YaTomado;
            }

            if (envio.Estado != EstadoEnvio.BuscandoConductor)
            {
                return ResultadoAceptar.NoCumple;
            }

            var detalle = await _oportunidades.ObtenerDetalleAsync(usuarioId, viajeId, envioId, hoy);
            if (detalle == null)
            {
                return ResultadoAceptar.NoCumple;
            }

            _context.Add(new Asignacion
            {
                EnvioId = envio.Id,
                ViajeId = viajeId,
                FechaViaje = detalle.FechaViaje,
                DesvioKm = detalle.DesvioKm,
                TiempoExtraMin = detalle.TiempoExtraMin,
                Ganancia = detalle.Pago,
                AsignadoEn = DateTime.Now
            });

            _context.Add(new EnvioEstadoHistorial
            {
                EnvioId = envio.Id,
                EstadoAnterior = envio.Estado,
                EstadoNuevo = EstadoEnvio.ConductorAsignado,
                Fecha = DateTime.Now
            });

            envio.Estado = EstadoEnvio.ConductorAsignado;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return ResultadoAceptar.YaTomado;
            }
            catch (DbUpdateException)
            {
                return ResultadoAceptar.YaTomado;
            }

            return ResultadoAceptar.Exito;
        }

        public Task<ResultadoAceptar> ConfirmarCombinacionAsync(int usuarioId, int viajeId, List<int> envioIds)
        {
            return ConfirmarCombinacionAsync(usuarioId, viajeId, envioIds, DateOnly.FromDateTime(DateTime.Today));
        }

        // Reserva varios envíos juntos: se asignan todos o ninguno
        public async Task<ResultadoAceptar> ConfirmarCombinacionAsync(int usuarioId, int viajeId, List<int> envioIds, DateOnly hoy)
        {
            envioIds = envioIds.Distinct().ToList();
            if (envioIds.Count == 0)
            {
                return ResultadoAceptar.NoCumple;
            }

            var viaje = await _context.Viajes
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == viajeId && v.UsuarioId == usuarioId && v.Activo);
            if (viaje == null)
            {
                return ResultadoAceptar.NoEncontrado;
            }

            var envios = await _context.Envios
                .Include(e => e.Asignacion)
                .Where(e => envioIds.Contains(e.Id))
                .ToListAsync();
            if (envios.Count != envioIds.Count)
            {
                return ResultadoAceptar.NoEncontrado;
            }

            if (envios.Any(e => e.Estado == EstadoEnvio.Cancelado))
            {
                return ResultadoAceptar.NoCumple;
            }

            // Si otro conductor se llevó aunque sea uno, no se reserva ninguno
            if (envios.Any(e => e.Estado == EstadoEnvio.ConductorAsignado || e.Asignacion != null))
            {
                return ResultadoAceptar.YaTomado;
            }

            if (envios.Any(e => e.Estado != EstadoEnvio.BuscandoConductor))
            {
                return ResultadoAceptar.NoCumple;
            }

            // Se vuelven a calcular las oportunidades para revalidar con los datos de este momento
            var oportunidades = await _oportunidades.BuscarAsync(usuarioId, viajeId, hoy);
            if (oportunidades == null)
            {
                return ResultadoAceptar.NoEncontrado;
            }

            var elegidas = oportunidades.Where(o => envioIds.Contains(o.EnvioId)).ToList();
            if (elegidas.Count != envioIds.Count)
            {
                return ResultadoAceptar.NoCumple;
            }

            // Juntos tienen que viajar el mismo día y entrar en el lugar que queda
            var fecha = elegidas[0].FechaViaje;
            bool mismoDia = elegidas.All(o => o.FechaViaje == fecha);
            bool entraPorCantidad = elegidas.Count <= elegidas[0].PaquetesDisponibles;
            bool entraPorPeso = elegidas.Sum(o => o.PesoKg) <= elegidas[0].PesoDisponibleKg;
            bool entraPorDesvio = elegidas.Sum(o => o.DesvioKm) <= elegidas[0].DesvioDisponibleKm;

            if (!mismoDia || !entraPorCantidad || !entraPorPeso || !entraPorDesvio)
            {
                return ResultadoAceptar.NoCumple;
            }

            foreach (var oportunidad in elegidas)
            {
                var envio = envios.First(e => e.Id == oportunidad.EnvioId);

                _context.Add(new Asignacion
                {
                    EnvioId = envio.Id,
                    ViajeId = viajeId,
                    FechaViaje = fecha,
                    DesvioKm = oportunidad.DesvioKm,
                    TiempoExtraMin = oportunidad.TiempoExtraMin,
                    Ganancia = oportunidad.Pago,
                    AsignadoEn = DateTime.Now
                });

                _context.Add(new EnvioEstadoHistorial
                {
                    EnvioId = envio.Id,
                    EstadoAnterior = envio.Estado,
                    EstadoNuevo = EstadoEnvio.ConductorAsignado,
                    Fecha = DateTime.Now
                });

                envio.Estado = EstadoEnvio.ConductorAsignado;
            }

            // Un solo SaveChanges: la base guarda todo junto o no guarda nada
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return ResultadoAceptar.YaTomado;
            }
            catch (DbUpdateException)
            {
                return ResultadoAceptar.YaTomado;
            }

            return ResultadoAceptar.Exito;
        }
    }
}
