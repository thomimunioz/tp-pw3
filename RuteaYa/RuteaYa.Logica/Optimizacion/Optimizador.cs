namespace RuteaYa.Logica.Optimizacion;

public static class Optimizador
{
    private const int MAX_CANDIDATOS = 15;
    private const int MAX_PARA_MOSTRAR = 2;

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

        // Separar las combinaciones en válidas y descartadas
        var validas = new List<Combinacion>();
        var descartadas = new List<Combinacion>();
        foreach (var envios in combinaciones)
        {
            if (envios.Count == 0)
            {
                continue;
            }

            var combinacion = new Combinacion
            {
                Envios = envios,
                GananciaTotal = envios.Sum(c => c.Pago),
                PesoTotalKg = envios.Sum(c => c.PesoKg),
                DesvioTotalKm = envios.Sum(c => c.DesvioKm)
            };
            combinacion.MotivoDescarte = MotivoDescarte(combinacion, maxPaquetes, pesoMaxKg, desvioMaxKm);

            if (combinacion.MotivoDescarte == null)
            {
                validas.Add(combinacion);
            }
            else
            {
                descartadas.Add(combinacion);
            }
        }

        // Mayor ganancia primero; si empatan, la de menor desvío
        var ordenadas = validas
            .OrderByDescending(c => c.GananciaTotal)
            .ThenBy(c => c.DesvioTotalKm)
            .ToList();

        var resultado = new ResultadoOptimizacion();
        var mejor = ordenadas.FirstOrDefault();
        if (mejor != null)
        {
            resultado.Seleccionados = mejor.Envios;
            resultado.GananciaTotal = mejor.GananciaTotal;
            resultado.PesoTotalKg = mejor.PesoTotalKg;
            resultado.DesvioTotalKm = mejor.DesvioTotalKm;
        }

        resultado.Alternativas = ordenadas.Skip(1).Take(MAX_PARA_MOSTRAR).ToList();

        // Solo interesan las que pagarían más que la elegida y no se pasan de paquetes
        resultado.Descartadas = descartadas
            .Where(c => c.Envios.Count <= maxPaquetes && c.GananciaTotal > resultado.GananciaTotal)
            .OrderByDescending(c => c.GananciaTotal)
            .Take(MAX_PARA_MOSTRAR)
            .ToList();

        return resultado;
    }

    private static string? MotivoDescarte(Combinacion combinacion, int maxPaquetes, decimal pesoMaxKg, decimal desvioMaxKm)
    {
        if (combinacion.Envios.Count > maxPaquetes)
        {
            return $"Supera los {maxPaquetes} paquetes";
        }
        if (combinacion.PesoTotalKg > pesoMaxKg)
        {
            return $"Supera los {pesoMaxKg:0.##} kg";
        }
        if (combinacion.DesvioTotalKm > desvioMaxKm)
        {
            return $"Supera los {desvioMaxKm:0.##} km de desvío";
        }
        return null;
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
