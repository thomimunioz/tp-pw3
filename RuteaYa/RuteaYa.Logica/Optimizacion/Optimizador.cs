namespace RuteaYa.Logica.Optimizacion;

public static class Optimizador
{
    private const int MAX_CANDIDATOS = 15;
    public static ResultadoOptimizacion Optimizar(
        List<CandidatoEnvio> candidatos,
        int maxPaquetes,
        decimal pesoMaxKg,
        decimal desvioMaxKm)
    {
        if (candidatos.Count > MAX_CANDIDATOS)
        {
            candidatos = candidatos.OrderByDescending(c => c.Pago).Take(MAX_CANDIDATOS).ToList();
        }
        // Generar todas las combinaciones posibles de candidatos
        var combinaciones = new List<List<CandidatoEnvio>>();
        GenerarCombinaciones(candidatos, 0, new List<CandidatoEnvio>(), combinaciones);
        var mejorCombinacion = new ResultadoOptimizacion();
        foreach (var combinacion in combinaciones)
        {
            if (combinacion.Count <= maxPaquetes)
            {
                var pesoTotal = combinacion.Sum(c => c.PesoKg);
                var desvioTotal = combinacion.Sum(c => c.DesvioKm);
                if (pesoTotal <= pesoMaxKg && desvioTotal <= desvioMaxKm)
                {
                    var gananciaTotal = combinacion.Sum(c => c.Pago);
                    if (gananciaTotal > mejorCombinacion.GananciaTotal || (gananciaTotal == mejorCombinacion.GananciaTotal && desvioTotal < mejorCombinacion.DesvioTotalKm))
                    {
                        mejorCombinacion.Seleccionados = combinacion;
                        mejorCombinacion.GananciaTotal = gananciaTotal;
                        mejorCombinacion.PesoTotalKg = pesoTotal;
                        mejorCombinacion.DesvioTotalKm = desvioTotal;
                    }
                }
            }
        }
        return mejorCombinacion;
    }
    
    private static void GenerarCombinaciones(
        List<CandidatoEnvio> candidatos,
        int indice,
        List<CandidatoEnvio> elegidos,
        List<List<CandidatoEnvio>> combinaciones)
    {
        if (indice == candidatos.Count)
        {
            combinaciones.Add(elegidos);
            return;
        }
        var elegidosSiLoLlevo = new List<CandidatoEnvio>(elegidos);
        elegidosSiLoLlevo.Add(candidatos[indice]);
        GenerarCombinaciones(candidatos, indice +1, elegidosSiLoLlevo, combinaciones);

        GenerarCombinaciones(candidatos, indice +1, elegidos, combinaciones);
    }
}