using RuteaYa.Logica.Optimizacion.Dtos;

namespace RuteaYa.Logica.Optimizacion;

public interface IOptimizadorServicios
{
    Task<OptimizacionDto?> CalcularAsync(int usuarioId, int viajeId);
    Task<OptimizacionDto?> CalcularAsync(int usuarioId, int viajeId, DateOnly hoy);
}
