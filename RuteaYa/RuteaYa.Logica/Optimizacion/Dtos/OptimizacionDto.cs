namespace RuteaYa.Logica.Optimizacion.Dtos;

public class OptimizacionDto
{
    public int ViajeId { get; set; }
    public string Origen { get; set; } = "";
    public string Destino { get; set; } = "";
    public int MaxPaquetes { get; set; }
    public decimal PesoMaxKg { get; set; }
    public decimal DesvioMaxKm { get; set; }

    // Fecha para la que se arma la combinación (null = no hay envíos candidatos)
    public DateOnly? FechaViaje { get; set; }

    // Lugar que le queda libre al viaje ese día
    public int PaquetesDisponibles { get; set; }
    public decimal PesoDisponibleKg { get; set; }
    public decimal DesvioDisponibleKm { get; set; }

    public List<CandidatoEnvio> Candidatos { get; set; } = new List<CandidatoEnvio>();
    public ResultadoOptimizacion Resultado { get; set; } = new ResultadoOptimizacion();
}
