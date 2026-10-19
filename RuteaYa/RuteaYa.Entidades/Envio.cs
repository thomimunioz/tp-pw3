using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades.Enums;

namespace RuteaYa.Entidades;

public class Envio
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

    public DateOnly FechaLimite { get; set; }
    [Precision(8, 2)]
    public decimal PesoKg { get; set; }
    public TamanoPaquete Tamano { get; set; }

    [MaxLength(500)]
    public string Descripcion { get; set; } = null!;
    [Precision(12, 2)]
    public decimal Monto { get; set; }

    public EstadoEnvio Estado { get; set; } = EstadoEnvio.BuscandoConductor;
    public DateTime CreadoEn { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    public Asignacion? Asignacion { get; set; }
    public ICollection<EnvioEstadoHistorial> Historial { get; set; } = new List<EnvioEstadoHistorial>();
}
