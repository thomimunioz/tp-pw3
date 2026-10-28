using RuteaYa.Entidades;

namespace RuteaYa.Web.ViewModels;

public class ViajeDetalleViewModel
{
    public int Id { get; set; }
    public string OrigenDir { get; set; } = string.Empty;
    public string DestinoDir { get; set; } = string.Empty;
    public string HoraSalida { get; set; } = string.Empty;
    public string HoraLlegada { get; set; } = string.Empty;
    public int MaxPaquetes { get; set; }
    public decimal PesoMaxKg { get; set; }
    public decimal DesvioMaxKm { get; set; }
    public decimal DistanciaKm { get; set; }
    public int DuracionMin { get; set; }
    public bool Activo { get; set; }
    public List<string> Dias { get; set; } = new();
    public List<Parada> Paradas { get; set; } = new();
    public List<PaqueteAsignadoViewModel> PaquetesAsignados { get; set; } = new();
}

public class PaqueteAsignadoViewModel
{
    public int Id { get; set; }
    public string OrigenDir { get; set; } = string.Empty;
    public string DestinoDir { get; set; } = string.Empty;
    public decimal PesoKg { get; set; }
    public string Tamano { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateOnly FechaViaje { get; set; }
}
