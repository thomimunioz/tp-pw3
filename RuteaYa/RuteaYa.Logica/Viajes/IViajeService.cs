using RuteaYa.Entidades;
using RuteaYa.Logica.Servicios.Dtos;

namespace RuteaYa.Logica.Viajes;

public interface IViajeServicios
{
    Task<List<Viaje>> ObtenerPorUsuarioAsync(int usuarioId);
    Task<Viaje?> ObtenerDetallePorUsuarioAsync(int viajeId, int usuarioId);
    Task<ViajeRecorridoDto?> ObtenerRecorridoPorUsuarioAsync(int viajeId, int usuarioId);
    Task CrearViajeAsync(Viaje viaje, List<int> diasSeleccionados, string? paradasIntermedias);
    Task<Viaje?> ObtenerPorIdAsync(int id, int usuarioId);
    Task EditarViajeAsync(Viaje viaje, List<int> diasSeleccionados, string? paradasIntermedias);
}