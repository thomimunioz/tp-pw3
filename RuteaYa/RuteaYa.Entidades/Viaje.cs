using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace RuteaYa.Entidades;

public class Viaje
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    [MaxLength(250)]
    public string OrigenDir { get; set; } = null!;
    [Precision(9, 6)]
    public decimal OrigenLat { get; set; }
    [Precision(9, 6)]
    public decimal OrigenLng { get; set; }

    [MaxLength(250)]
    public string DestinoDir { get; set; } = null!;
    [Precision(9, 6)]
    public decimal DestinoLat { get; set; }
    [Precision(9, 6)]
    public decimal DestinoLng { get; set; }

    public TimeOnly HoraSalida { get; set; }
    public TimeOnly HoraLlegada { get; set; }

    public int MaxPaquetes { get; set; }
    [Precision(8, 2)]
    public decimal PesoMaxKg { get; set; }
    [Precision(8, 2)]
    public decimal DesvioMaxKm { get; set; }

    [Precision(8, 2)]
    public decimal DistanciaKm { get; set; }
    public int DuracionMin { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<Parada> Paradas { get; set; } = new List<Parada>();
    public ICollection<ViajeDia> Dias { get; set; } = new List<ViajeDia>();
    public ICollection<Asignacion> Asignaciones { get; set; } = new List<Asignacion>();
}
