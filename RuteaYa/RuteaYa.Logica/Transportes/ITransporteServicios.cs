using RuteaYa.Logica.Transportes.Dtos;

namespace RuteaYa.Logica.Transportes
    {

    public interface ITransporteServicios
        {

        Task<List<TransporteListadoDto>> ListarMisTransportesAsync(int conductorId);
        Task<ResultadoActualizarTransporte> MarcarRetiradoAsync(int conductorId, int envioId);
        Task<ResultadoActualizarTransporte> MarcarEnCaminoAsync(int conductorId, int envioId);
        Task<ResultadoActualizarTransporte> MarcarEntregadoAsync(int conductorId, int envioId);
        }
    }