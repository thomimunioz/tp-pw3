namespace RuteaYa.Logica.Optimizacion;

public class ProyeccionMensualDto
{
    public DateOnly Mes { get; set; }
    public CriterioOptimizacion Criterio { get; set; }
    public int ViajesAnalizados { get; set; }
    public int PaquetesPosibles { get; set; }
    public decimal GananciaPotencial { get; set; }
    public decimal PromedioPorViaje { get; set; }
    public List<ProyeccionSemanaDto> Semanas { get; set; } = new();
    public List<ProyeccionViajeDto> Viajes { get; set; } = new();
    public ProyeccionViajeDto? MejorRecorrido { get; set; }
}

public class ProyeccionSemanaDto
{
    public DateOnly Desde { get; set; }
    public DateOnly Hasta { get; set; }
    public decimal GananciaPotencial { get; set; }
}

public class ProyeccionViajeDto
{
    public int ViajeId { get; set; }
    public DateOnly Fecha { get; set; }
    public string Origen { get; set; } = "";
    public string Destino { get; set; } = "";
    public decimal GananciaPotencial { get; set; }
    public decimal DesvioTotalKm { get; set; }
    public List<ProyeccionEnvioDto> Envios { get; set; } = new();
}

public class ProyeccionEnvioDto
{
    public int EnvioId { get; set; }
    public string Descripcion { get; set; } = "";
    public string Ruta { get; set; } = "";
    public decimal Ganancia { get; set; }
    public decimal PesoKg { get; set; }
    public decimal DesvioKm { get; set; }
}
