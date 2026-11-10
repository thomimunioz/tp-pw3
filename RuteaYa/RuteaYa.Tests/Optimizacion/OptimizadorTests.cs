using RuteaYa.Logica.Optimizacion;
namespace RuteaYa.Tests.Optimizacion;

public class OptimizadorTests
{
    private static CandidatoEnvio Candidato(int id, decimal pago, decimal peso, decimal desvio)
    {
    return new CandidatoEnvio { EnvioId = id, Pago = pago, PesoKg = peso, DesvioKm = desvio };
    }

    [Fact]
    public void Optimizar_UnEnvioQuePesaElLimite_LoElige()
    {
        // Arrange
        var candidatos = new List<CandidatoEnvio>
        {
            Candidato(1, 1000m, 10m, 1m)
        };

        // Act
        var resultado = Optimizador.Optimizar(candidatos, maxPaquetes: 3, pesoMaxKg: 10m, desvioMaxKm: 5m);

        // Assert
        var elegido = Assert.Single(resultado.Seleccionados);
        Assert.Equal(1, elegido.EnvioId);
    }

    [Fact]
    public void Optimizar_NingunEnvioEntraPorPeso_DevuelveCombinacionVacia()
    {
        //Arrange
        var candidatos = new List<CandidatoEnvio>
        {
            Candidato(1, 1000m, 11m, 1m),
            Candidato(2, 2000m, 12m, 2m)
        };

        //Act
        var resultado = Optimizador.Optimizar(candidatos, maxPaquetes: 3, pesoMaxKg: 10m, desvioMaxKm: 5m);

        //Assert
        Assert.Empty(resultado.Seleccionados);
        Assert.Equal(0m, resultado.GananciaTotal);
    }

    [Fact]
    public void Optimizar_EjemploDelEnunciado_EligeABEConDoceMil()
    {
        //Arrange
        var candidatos = new List<CandidatoEnvio>
        {
            Candidato(1, 4000m, 2m, 1m), // A
            Candidato(2, 5500m, 6m, 2m), // B
            Candidato(3, 3500m, 4m, 1m), // C
            Candidato(4, 7000m, 8m, 3m), // D
            Candidato(5, 2500m, 1m, 0.5m)  // E
        };

        //Act
        var resultado = Optimizador.Optimizar(candidatos, maxPaquetes: 3, pesoMaxKg: 10m, desvioMaxKm: 5m);

        //Assert
        var ids = resultado.Seleccionados.Select(c => c.EnvioId).ToList();
        Assert.Equal(new List<int> { 1, 2, 5 }, ids);
        Assert.Equal(12000m, resultado.GananciaTotal);
        Assert.Equal(9m, resultado.PesoTotalKg);
        Assert.Equal(3.5m, resultado.DesvioTotalKm);
    }

    [Fact]
    public void Optimizar_DosCombinacionesConLaMismaGanancia_DevuelveLaQueDesviaMenos()
    {
        //Arrange
        var candidatos = new List<CandidatoEnvio>
        {
            Candidato(1, 5000m, 1m, 3m),
            Candidato(2, 5000m, 1m, 1m)
        };

        //Act
        var resultado = Optimizador.Optimizar(candidatos, maxPaquetes: 1, pesoMaxKg: 10m, desvioMaxKm: 10m);

        //Assert
        var elegido = Assert.Single(resultado.Seleccionados);
        Assert.Equal(2, elegido.EnvioId);
    }

    [Fact]
    public void Optimizar_MasDeQuinceCandidatos_SoloConsideraLosQuinceQueMasPagan()
    {
        // Arrange
        var candidatos = new List<CandidatoEnvio>();
        for (int i = 1; i <= 20; i++)
        {
            candidatos.Add(Candidato(i, i * 100m, 1m, 0.1m));
        }

        // Act
        var resultado = Optimizador.Optimizar(candidatos, maxPaquetes: 20, pesoMaxKg: 100m, desvioMaxKm: 100m);

        // Assert
        Assert.Equal(15, resultado.Seleccionados.Count);
        Assert.DoesNotContain(resultado.Seleccionados, c => c.EnvioId <= 5);
    }

}