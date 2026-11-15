using RuteaYa.Logica.Optimizacion.Dtos;
using RuteaYa.Logica.Viajes;

namespace RuteaYa.Web.Models;

public class OptimizadorViewModel
{
    public int ViajeId { get; set; }
    public OptimizacionDto Optimizacion { get; set; } = new();

    public List<ViajeOpcionDto> Viajes { get; set; } = new();
    public bool SinViajes => Viajes.Count == 0;
}
