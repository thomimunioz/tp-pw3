namespace RuteaYa.Logica.Servicios.Dtos;

/// <summary>
/// Datos mínimos necesarios para representar el recorrido de un viaje en el mapa.
/// </summary>
public class ViajeRecorridoDto
{
    public int Id { get; set; }
    public PuntoRecorridoDto Origen { get; set; } = new();
    public PuntoRecorridoDto Destino { get; set; } = new();
    public List<PuntoRecorridoDto> Paradas { get; set; } = new();
}

public class PuntoRecorridoDto
{
    public string Direccion { get; set; } = string.Empty;
    public decimal Latitud { get; set; }
    public decimal Longitud { get; set; }
    public int? Orden { get; set; }
}
