using Microsoft.EntityFrameworkCore;

namespace RuteaYa.Entidades;

[PrimaryKey(nameof(ViajeId), nameof(DiaSemana))]
public class ViajeDia
{
    public int ViajeId { get; set; }
    public Viaje Viaje { get; set; } = null!;

    public DayOfWeek DiaSemana { get; set; }
}
