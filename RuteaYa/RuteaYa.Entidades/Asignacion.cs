using Microsoft.EntityFrameworkCore;

namespace RuteaYa.Entidades;

public class Asignacion
{
    public int Id { get; set; }

    public int EnvioId { get; set; }
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Envio Envio { get; set; } = null!;

    public int ViajeId { get; set; }
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Viaje Viaje { get; set; } = null!;

    public DateOnly FechaViaje { get; set; }
    [Precision(8, 2)]
    public decimal DesvioKm { get; set; }
    public int TiempoExtraMin { get; set; }
    [Precision(12, 2)]
    public decimal Ganancia { get; set; }
    public DateTime AsignadoEn { get; set; }
}
