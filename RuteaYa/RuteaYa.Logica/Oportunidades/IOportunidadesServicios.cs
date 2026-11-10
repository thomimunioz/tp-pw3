using RuteaYa.Logica.Oportunidades.Dtos;

namespace RuteaYa.Logica.Oportunidades;

public interface IOportunidadesServicios
{
    Task<List<OportunidadDto>?> BuscarAsync(int usuarioId, int viajeId);
    Task<List<OportunidadDto>?> BuscarAsync(int usuarioId, int viajeId, DateOnly hoy);
    Task<OportunidadDto?> ObtenerDetalleAsync(int usuarioId, int viajeId, int envioId);
    Task<OportunidadDto?> ObtenerDetalleAsync(int usuarioId, int viajeId, int envioId, DateOnly hoy);
}
