using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace RuteaYa.Entidades;

public class Usuario : IdentityUser<int>
{
    [MaxLength(100)]
    public string Nombre { get; set; } = null!;

    [MaxLength(100)]
    public string Apellido { get; set; } = null!;

    public DateTime CreadoEn { get; set; }

    public ICollection<Viaje> Viajes { get; set; } = new List<Viaje>();
    public ICollection<Envio> Envios { get; set; } = new List<Envio>();
}
