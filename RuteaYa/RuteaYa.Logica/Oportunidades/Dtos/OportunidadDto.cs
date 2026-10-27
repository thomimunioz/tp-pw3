namespace RuteaYa.Logica.Oportunidades.Dtos;

public class OportunidadDto
{
    public int EnvioId { get; set; }
    public string OrigenDireccion { get; set; } = "";
    public string DestinoDireccion { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public decimal Pago { get; set; }
    public decimal PesoKg { get; set; }
    public DateOnly FechaViaje { get; set; }
    public decimal DistanciaHabitualKm { get; set; }
    public decimal DistanciaConEnvioKm { get; set; }
    public decimal DesvioKm { get; set; }
    public int TiempoExtraMin { get; set; }
    public decimal GananciaPorKm { get; set; }
    public int TiempoHabitualMin { get; set; }
    public int PaquetesDisponibles { get; set; }
    public decimal PesoDisponibleKg { get; set; }
}