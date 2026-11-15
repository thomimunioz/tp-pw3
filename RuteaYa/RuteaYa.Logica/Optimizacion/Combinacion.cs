namespace RuteaYa.Logica.Optimizacion;

public class Combinacion
{
    public List<CandidatoEnvio> Envios { get; set; } = new List<CandidatoEnvio>();
    public decimal GananciaTotal { get; set; }
    public decimal PesoTotalKg { get; set; }
    public decimal DesvioTotalKm { get; set; }

    // null = la combinación respeta todas las restricciones del viaje
    public string? MotivoDescarte { get; set; }
}
