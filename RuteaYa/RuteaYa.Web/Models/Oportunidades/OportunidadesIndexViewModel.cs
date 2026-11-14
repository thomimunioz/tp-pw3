using RuteaYa.Logica.Oportunidades.Dtos;
using RuteaYa.Logica.Viajes;

namespace RuteaYa.Web.Models;

public class OportunidadesIndexViewModel
{
    public int ViajeId { get; set; }
    public string Titulo { get; set; } = "";
    public OrdenOportunidades Orden { get; set; }
    public List<OportunidadDto> Oportunidades { get; set; } = new();

    public List<ViajeOpcionDto> Viajes { get; set; } = new();
    public bool SinViajes => Viajes.Count == 0;
}