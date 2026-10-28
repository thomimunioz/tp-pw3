using RuteaYa.Entidades;

namespace RuteaYa.Logica.Viajes;

public interface IViajeServicios
{
    Task<List<Viaje>> ObtenerPorUsuarioAsync(int usuarioId);
    Task<Viaje?> ObtenerDetallePorUsuarioAsync(int viajeId, int usuarioId);
    Task CrearViajeAsync(Viaje viaje, List<int> diasSeleccionados, string? paradasIntermedias);
}



