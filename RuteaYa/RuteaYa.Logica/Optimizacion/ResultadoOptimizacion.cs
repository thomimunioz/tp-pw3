namespace RuteaYa.Logica.Optimizacion;

public class ResultadoOptimizacion
{
    public List<CandidatoEnvio> Seleccionados { get; set; } = new List<CandidatoEnvio>();
    public decimal GananciaTotal { get; set; }
    public decimal PesoTotalKg { get; set; }
    public decimal DesvioTotalKm { get; set; }
}