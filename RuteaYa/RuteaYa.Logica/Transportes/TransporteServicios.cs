using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Logica.Transportes.Dtos;

namespace RuteaYa.Logica.Transportes
    {

    public class TransporteServicios : ITransporteServicios
        {

        private readonly RuteaYaContext _context;

        public TransporteServicios(RuteaYaContext context)
            {
            this._context = context;
            }

        public async Task<List<TransporteListadoDto>> ListarMisTransportesAsync(int conductorId)
            {

            var transportes = await this._context.Asignaciones.Include(a => a.Viaje).Include(a => a.Envio).Where(a => a.Viaje.UsuarioId == conductorId).OrderByDescending(a => a.AsignadoEn).ToListAsync();

            return transportes.Select(a => new TransporteListadoDto
                {
                AsignacionId = a.Id,
                EnvioId = a.EnvioId,
                ViajeId = a.ViajeId,
                ViajeOrigen = a.Viaje.OrigenDir,
                ViajeDestino = a.Viaje.DestinoDir,
                FechaViaje = a.FechaViaje,
                PaqueteDescripcion = a.Envio.Descripcion,
                PaqueteOrigen = a.Envio.OrigenDir,
                PaqueteDestino = a.Envio.DestinoDir,
                PesoKg = a.Envio.PesoKg,
                Tamano = a.Envio.Tamano,
                Ganancia = a.Ganancia,
                Estado = a.Envio.Estado
                }).ToList();
            }

        public Task<ResultadoActualizarTransporte> MarcarRetiradoAsync(int conductorId, int envioId)
            {
                return CambiarEstadoAsync(conductorId, envioId, estadoEsperado: EstadoEnvio.ConductorAsignado, nuevoEstado: EstadoEnvio.Retirado, mensajeErrorEstadoInvalido: "Solo se puede marcar como retirado un paquete que esté en estado 'Conductor asignado'.");
            }


        public Task<ResultadoActualizarTransporte> MarcarEnCaminoAsync(int conductorId, int envioId)
            {
                return CambiarEstadoAsync(conductorId, envioId, estadoEsperado: EstadoEnvio.Retirado, nuevoEstado: EstadoEnvio.EnCamino, mensajeErrorEstadoInvalido: "Solo se puede marcar en camino un paquete que ya haya sido retirado previamente.");
            }


        public Task<ResultadoActualizarTransporte> MarcarEntregadoAsync(int conductorId, int envioId)
            {
                return CambiarEstadoAsync(conductorId, envioId, estadoEsperado: EstadoEnvio.EnCamino, nuevoEstado: EstadoEnvio.Entregado, mensajeErrorEstadoInvalido: "No se puede marcar como entregado un paquete que no fue retirado ni puesto en camino.");
            }


        private async Task<ResultadoActualizarTransporte> CambiarEstadoAsync(int conductorId, int envioId, EstadoEnvio estadoEsperado, EstadoEnvio nuevoEstado, string mensajeErrorEstadoInvalido)
            {

            var asignacion = await _context.Asignaciones.Include(a => a.Viaje).Include(a => a.Envio).ThenInclude(e => e.Historial).FirstOrDefaultAsync(a => a.EnvioId == envioId && a.Viaje.UsuarioId == conductorId);
            if (asignacion == null)
                {
                return ResultadoActualizarTransporte.NoExiste();
                }

            var envio = asignacion.Envio;
            if (envio.Estado == EstadoEnvio.Entregado)
                {
                return ResultadoActualizarTransporte.Fallido("El paquete ya fue entregado y la operación está finalizada.");
                }

            if (envio.Estado == EstadoEnvio.Cancelado)
                {
                return ResultadoActualizarTransporte.Fallido("El paquete fue cancelado y no se puede transportar.");
                }

            if (envio.Estado != estadoEsperado)
                {
                return ResultadoActualizarTransporte.Fallido(mensajeErrorEstadoInvalido);
                }

            var estadoAnterior = envio.Estado;
            envio.Estado = nuevoEstado;

            envio.Historial.Add(new EnvioEstadoHistorial
                {
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = nuevoEstado,
                Fecha = DateTime.Now
                });

            try
                {
                await this._context.SaveChangesAsync();
                } catch (DbUpdateConcurrencyException)
                {
                return ResultadoActualizarTransporte.Fallido("El estado del paquete cambió en otro proceso. Actualizá la pagína e intentá nuevamente.");
                }

            return ResultadoActualizarTransporte.Exitoso();
            }
        }
    }