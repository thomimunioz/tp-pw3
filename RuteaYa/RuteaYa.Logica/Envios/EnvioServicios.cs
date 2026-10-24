using RuteaYa.Logica.Mapas;
using System;
using System.Collections.Generic;
using System.Text;
using RuteaYa.Entidades.Enums;
using RuteaYa.Logica.Envios.Dtos;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;

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

            if(dto.Monto <= 0)
            {
                errores.Add("El monto ofrecido debe ser mayor a $0.");
            }

            return errores;
        }
    }
}