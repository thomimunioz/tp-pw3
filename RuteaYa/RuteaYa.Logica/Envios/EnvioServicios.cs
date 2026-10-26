using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Entidades.Enums;
using RuteaYa.Logica.Envios.Dtos;
using RuteaYa.Logica.Mapas;
using System;
using System.Collections.Generic;
using System.Text;

namespace RuteaYa.Logica.Envios {

    public class EnvioServicios : IEnvioServicios
        {

        private readonly RuteaYaContext _context;
        private readonly IGeocodificador _geocodificador;

        public EnvioServicios(RuteaYaContext context, IGeocodificador geocodificador){
            _context = context;
            _geocodificador = geocodificador;
        }

        public async Task<ResultadoCrearEnvio> CrearAsync(int usuarioId, CrearEnvioDto dto)
        {
            var errores = Validar(dto);
            if(errores.Count > 0)
            {
                return new ResultadoCrearEnvio
                {
                    ResultadoExitoso = false,
                    Errores = errores
                };
            }

            // Geocodificación.
            var origenCoordenadas = await _geocodificador.GeocodificarAsync(dto.OrigenDirrecion.Trim());
            if (origenCoordenadas == null)
            {
                return ResultadoCrearEnvio.Fallido(new[]
                {
                    "No se pudo ubicar la dirección de origen en el mapa. Verificá que sea correcta."
                });
            }

            var destinoCoordenadas = await _geocodificador.GeocodificarAsync(dto.DestinoDireccion.Trim());
            if (destinoCoordenadas == null)
            {
                return ResultadoCrearEnvio.Fallido(new[]
                {
                "No se pudo ubicar la dirección de destino en el mapa. Verificá que sea correcta."
                });
            }


            var ahora = DateTime.Now;

            var envio = new Envio {
                UsuarioId = usuarioId,
                OrigenDir = dto.OrigenDirrecion.Trim(),
                OrigenLat = origenCoordenadas.Lat,
                OrigenLng = origenCoordenadas.Lng,
                DestinoDir = dto.DestinoDireccion.Trim(),
                DestinoLat = destinoCoordenadas.Lat,
                DestinoLng = destinoCoordenadas.Lng,
                FechaLimite = dto.FechaLimite,
                PesoKg = dto.Peso,
                Tamano = dto.Tamano,
                Descripcion = dto.Descripcion.Trim(),
                Monto = dto.Monto,
                Estado = EstadoEnvio.BuscandoConductor,
                CreadoEn = ahora,
                RowVersion = new byte[8]
                };

            envio.Historial.Add(new EnvioEstadoHistorial {
                EstadoAnterior = null,
                EstadoNuevo = EstadoEnvio.BuscandoConductor,
                Fecha = ahora
            });

            _context.Envios.Add(envio);
            await _context.SaveChangesAsync();

            return ResultadoCrearEnvio.Exitoso(envio.Id);
        }



        private static List<string> Validar(CrearEnvioDto dto)
            {
            var errores = new List<string>();

            if (dto.OrigenDirrecion == null || dto.OrigenDirrecion.Trim() == "")
            {
                errores.Add("El origen es obligatorio.");
            }

            if (dto.DestinoDireccion == null || dto.DestinoDireccion.Trim().Length == 0)
            {
                errores.Add("El destino es obligatorio.");
            }

            if (dto.OrigenDirrecion != null && dto.OrigenDirrecion.Trim().Length > 0 &&
               dto.DestinoDireccion != null && dto.DestinoDireccion.Trim().Length > 0 &&
               dto.OrigenDirrecion.Trim().Equals(dto.DestinoDireccion.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                errores.Add("El origen y el destino deben ser diferentes.");
            }

            var hoy = DateOnly.FromDateTime(DateTime.Today);

            if (dto.FechaLimite < hoy)
            {
                errores.Add("La fecha no puede ser anterior a la fecha actual.");
            }

            if (dto.Peso <= 0)
            {
                errores.Add("El peso debe ser mayor a 0 kg.");
            }

            if(Enum.IsDefined(typeof(TamanoPaquete), dto.Tamano) == false)
            {
                errores.Add("El tamaño del paquete es obligatorio.");
            }

            if (dto.Descripcion == null || dto.Descripcion.Trim() == "")
            {
                errores.Add("La descripción es obligatoria.");
            } else if(dto.Descripcion.Trim().Length > 500)
              {
                errores.Add("La descripción no puede superar los 500 caracteres.");
              }

            if(dto.Monto < 1)
            {
                errores.Add("El monto ofrecido debe ser mayor a $0.");
            }

            return errores;
        }
        public async Task<List<EnvioListadoDto>> ListarAsync(int usuarioId, EstadoEnvio? estado)
        {
            var consulta = _context.Envios
                .AsNoTracking()             //solo va a leer
                .Where(e => e.UsuarioId == usuarioId); //envios del usuario

            if (estado.HasValue)
            {
                consulta = consulta.Where(e => e.Estado == estado.Value);
            }

            var envios = await consulta
                .OrderByDescending(e => e.CreadoEn)
                .ToListAsync();

            return envios.Select(e => new EnvioListadoDto
            {
                Id = e.Id,
                OrigenDir = e.OrigenDir,
                DestinoDir = e.DestinoDir,
                Descripcion = e.Descripcion,
                Monto = e.Monto,
                Estado = e.Estado,
                FechaLimite = e.FechaLimite,
                PuedeCancelar = PuedeCancelar(e.Estado)
            }).ToList();
        }

        private static bool PuedeCancelar(EstadoEnvio estado)
        {
            return estado == EstadoEnvio.BuscandoConductor
                || estado == EstadoEnvio.ConductorAsignado;
        }

        public async Task<EnvioDetalleDto?> ObtenerDetalleAsync(int usuarioId, int envioId)
        {
            var envio = await _context.Envios
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == envioId && e.UsuarioId == usuarioId);

            if (envio == null)
            {
                return null;
            }

            return new EnvioDetalleDto
            {
                Id = envio.Id,
                OrigenDir = envio.OrigenDir,
                DestinoDir = envio.DestinoDir,
                Descripcion = envio.Descripcion,
                Monto = envio.Monto,
                PesoKg = envio.PesoKg,
                Tamano = envio.Tamano,
                Estado = envio.Estado,
                FechaLimite = envio.FechaLimite,
                CreadoEn = envio.CreadoEn,
                PuedeCancelar = PuedeCancelar(envio.Estado)
            };
        }
        public async Task<ResultadoCancelarEnvio> CancelarAsync(int usuarioId, int envioId)
        {
            var envio = await _context.Envios
                .Include(e => e.Asignacion)
                .FirstOrDefaultAsync(e => e.Id == envioId && e.UsuarioId == usuarioId);

            if (envio == null)
            {
                return ResultadoCancelarEnvio.NoExiste();
            }

            if (PuedeCancelar(envio.Estado) == false)
            {
                return ResultadoCancelarEnvio.Fallido(
                    "Solo se pueden cancelar envíos que estén buscando conductor o con conductor asignado.");
            }

            var estadoAnterior = envio.Estado;
            envio.Estado = EstadoEnvio.Cancelado;

            if (envio.Asignacion != null)
            {
                _context.Remove(envio.Asignacion);
            }

            envio.Historial.Add(new EnvioEstadoHistorial
            {
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = EstadoEnvio.Cancelado,
                Fecha = DateTime.Now
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return ResultadoCancelarEnvio.Fallido(
                    "El envío cambió mientras lo cancelabas (por ejemplo, un conductor lo aceptó). Revisá su estado e intentá de nuevo.");
            }

            return ResultadoCancelarEnvio.Exitoso();
        }
    }
}