using System.ComponentModel.DataAnnotations;

namespace RuteaYa.Api.Dtos;

// DTO de salida: lo que devuelve la API en formato JSON
public class ParadaDto
{
    public int Id { get; set; }
    public int ViajeId { get; set; }
    public int Orden { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public decimal Lat { get; set; }
    public decimal Lng { get; set; }
}

// DTO de entrada: lo que pide Swagger/cliente para crear una parada nueva
public class CrearParadaDto
{
    [Required]
    public int ViajeId { get; set; }

    [Required]
    public int Orden { get; set; }

    [Required]
    [MaxLength(250)]
    public string Direccion { get; set; } = string.Empty;

    public decimal Lat { get; set; }
    public decimal Lng { get; set; }
}