namespace RuteaYa.Logica.Mapas;

public static class CalculadoraDistancias
{
    private const double RadioTierraKm  = 6371;
    private const double FactorCalles = 1.3;
    private const decimal VelocidadPromedioKmH = 30;

    //Formula de Haversine
    private static double LineaRectaKm(Coordenada a, Coordenada b)
    {
        double lat1 = ARadianes(a.Lat);
        double lat2 = ARadianes(b.Lat);
        double difLat = ARadianes(b.Lat - a.Lat);
        double difLng = ARadianes(b.Lng - a.Lng);

        double h = Math.Sin(difLat / 2) * Math.Sin(difLat / 2)
                 + Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(difLng / 2) * Math.Sin(difLng / 2);

        return 2 * RadioTierraKm * Math.Asin(Math.Sqrt(h));
    }

    private static double ARadianes(decimal grados)
    {
        return (double)grados * Math.PI / 180;
    }

    public static decimal DistanciaRutaKm(List<Coordenada> puntos)
    {
        double distancia = 0;
        for (int i = 0; i < puntos.Count - 1; i++)
        {
            distancia += LineaRectaKm(puntos[i], puntos[i + 1]);
        }

        return Math.Round((decimal)distancia * (decimal)FactorCalles, 2);
    }

    public static decimal DesvioKm(List<Coordenada> rutaSinEnvio, List<Coordenada> rutaConEnvio)
    {
        decimal distanciaSinEnvio = DistanciaRutaKm(rutaSinEnvio);
        decimal distanciaConEnvio = DistanciaRutaKm(rutaConEnvio);

        return distanciaConEnvio - distanciaSinEnvio;
    }

    public static int MinutosDeViaje(decimal km)
    {
        decimal minutos = Math.Round(60 * (km / VelocidadPromedioKmH));
        return (int)minutos;
    }
}