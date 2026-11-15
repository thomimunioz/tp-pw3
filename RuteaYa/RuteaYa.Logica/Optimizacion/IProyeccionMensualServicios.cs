namespace RuteaYa.Logica.Optimizacion;

public interface IProyeccionMensualServicios
{
    Task<ProyeccionMensualDto> CalcularAsync(
        int usuarioId,
        DateOnly mes,
        CriterioOptimizacion criterio);

    Task<ProyeccionMensualDto> CalcularAsync(
        int usuarioId,
        DateOnly mes,
        CriterioOptimizacion criterio,
        DateOnly hoy);
}
