using RuteaYa.Logica.Oportunidades.Dtos;

namespace RuteaYa.Web.Models;

public class DetalleOportunidadViewModel
{
    public int ViajeId { get; set; }
    public OportunidadDto Oportunidad { get; set; } = null!;
}