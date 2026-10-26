using RuteaYa.Logica.Envios.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace RuteaYa.Logica.Envios {

    public interface IEnvioServicios {

        Task<ResultadoCrearEnvio> CrearAsync(int usuarioId, CrearEnvioDto dto);
        Task<List<EnvioListadoDto>> ListarAsync(int usuarioId, EstadoEnvio? estado);
        Task<EnvioDetalleDto?> ObtenerDetalleAsync(int usuarioId, int envioId);
        Task<ResultadoCancelarEnvio> CancelarAsync(int usuarioId, int envioId);
    }
}