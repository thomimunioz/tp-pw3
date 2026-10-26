namespace RuteaYa.Logica.Optimizacion;

public static class Optimizador
{
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