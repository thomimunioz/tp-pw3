using RuteaYa.Logica.Oportunidades.Dtos;

namespace RuteaYa.Web.Models;

public class OportunidadesIndexViewModel
{
    public int ViajeId { get; set; }
    public string Titulo { get; set; } = "";
    public OrdenOportunidades Orden { get; set; }
    public List<OportunidadDto> Oportunidades { get; set; } = new();
}