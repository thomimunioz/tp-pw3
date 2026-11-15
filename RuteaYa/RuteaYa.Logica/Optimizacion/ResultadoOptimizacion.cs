namespace RuteaYa.Logica.Optimizacion;

public class ResultadoOptimizacion
{
    public List<CandidatoEnvio> Seleccionados { get; set; } = new List<CandidatoEnvio>();
    public decimal GananciaTotal { get; set; }
    public decimal PesoTotalKg { get; set; }
    public decimal DesvioTotalKm { get; set; }

    // Las siguientes mejores combinaciones válidas (después de la recomendada)
    public List<Combinacion> Alternativas { get; set; } = new List<Combinacion>();

    // Combinaciones que pagan más que la recomendada pero rompen alguna restricción
    public List<Combinacion> Descartadas { get; set; } = new List<Combinacion>();
}
