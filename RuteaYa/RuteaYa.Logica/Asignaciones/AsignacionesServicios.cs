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
                AsignadoEn = DateTime.UtcNow
            });

            _context.Add(new EnvioEstadoHistorial
            {
                EnvioId = envio.Id,
                EstadoAnterior = envio.Estado,
                EstadoNuevo = EstadoEnvio.ConductorAsignado,
                Fecha = DateTime.UtcNow
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
    }
}
