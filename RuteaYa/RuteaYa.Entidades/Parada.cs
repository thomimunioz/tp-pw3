using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace RuteaYa.Entidades;

public class Parada
{
    public int Id { get; set; }

    public int ViajeId { get; set; }
    public Viaje Viaje { get; set; } = null!;

    public int Orden { get; set; }

    [MaxLength(250)]
    public string Direccion { get; set; } = null!;
    [Precision(9, 6)]
    public decimal Lat { get; set; }
    [Precision(9, 6)]
    public decimal Lng { get; set; }
}
