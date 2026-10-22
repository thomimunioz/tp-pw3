using RuteaYa.Logica.Mapas;

namespace RuteaYa.Tests.Mapas;

public class CalculadoraDistanciasTests
{
    [Fact]
    public void DistanciaRutaKm_UnGradoDeLatitud_Da144_55()
    {
        // Arrange
        var puntos = new List<Coordenada>
        {
            new Coordenada { Lat = 0m, Lng = 0m },
            new Coordenada { Lat = 1m, Lng = 0m }
        };

        // Act
        decimal distancia = CalculadoraDistancias.DistanciaRutaKm(puntos);

        // Assert
        Assert.Equal(144.55m, distancia);
    }

    [Fact]
    public void DistanciaRutaKm_UnSoloPunto_DaCero()
    {
        // Arrange
        var puntos = new List<Coordenada>
        {
            new Coordenada { Lat = 0m, Lng = 0m }
        };

        // Act
        decimal distancia = CalculadoraDistancias.DistanciaRutaKm(puntos);

        // Assert
        Assert.Equal(0m, distancia);
    }

    [Fact]
    public void DistanciaRutaKm_ObeliscoAPlazaDeMayo_DaDistanciaRealista()
    {
        // Arrange
        var puntos = new List<Coordenada>
        {
            new Coordenada { Lat = -34.6037389m, Lng = -58.3815704m }, // Obelisco
            new Coordenada { Lat = -34.6083m, Lng = -58.3700m } // Plaza de Mayo
        };

        // Act
        decimal distancia = CalculadoraDistancias.DistanciaRutaKm(puntos);

        // Assert
        Assert.InRange(distancia, 1.2m, 1.7m);
    }

    [Fact]
    public void DesvioKm_EnvioQueSaleYLlegaDondeElViaje_DaCero()
    {
        // Arrange
        var rutaSinEnvio = new List<Coordenada>
        {
            new Coordenada { Lat = 0m, Lng = 0m },
            new Coordenada { Lat = 1m, Lng = 0m }
        };

        var rutaConEnvio = new List<Coordenada>
        {
            new Coordenada { Lat = 0m, Lng = 0m },
            new Coordenada { Lat = 0m, Lng = 0m },
            new Coordenada { Lat = 1m, Lng = 0m },
            new Coordenada { Lat = 1m, Lng = 0m }
        };


        // Act
        decimal desvio = CalculadoraDistancias.DesvioKm(rutaSinEnvio, rutaConEnvio);

        // Assert
        Assert.Equal(0m, desvio);
    }

    [Fact]
    public void DesvioKm_EnvioFueraDelCamino_DaPositivo()
    {
        // Arrange
        var rutaSinEnvio = new List<Coordenada>
        {
            new Coordenada { Lat = 0m, Lng = 0m },
            new Coordenada { Lat = 1m, Lng = 0m }
        };

        var rutaConEnvio = new List<Coordenada>
        {
            new Coordenada { Lat = 0m, Lng = 0m },
            new Coordenada { Lat = 0.5m, Lng = 1m },
            new Coordenada { Lat = 1m, Lng = 0m }
        };

        // Act
        decimal desvio = CalculadoraDistancias.DesvioKm(rutaSinEnvio, rutaConEnvio);

        // Assert
        Assert.True(desvio > 0);
    }

    [Fact]
    public void MinutosDeViaje_15Km_Da30Minutos()
    {
        // Arrange
        decimal km = 15m;

        // Act
        int minutos = CalculadoraDistancias.MinutosDeViaje(km);

        // Assert
        Assert.Equal(30, minutos);
    }
}
