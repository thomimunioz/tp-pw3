namespace RuteaYa.Logica.Mapas;
public class GeocodificadorFalso : IGeocodificador
{
    public Task<Coordenada?> GeocodificarAsync(string direccion)
    {
        int suma = 0;
        foreach (char letra in direccion)
        {
            suma += letra;
        }

        var coordenada = new Coordenada
        {
            Lat = -34.60m - (suma % 100) / 1000m,
            Lng = -58.38m - (suma % 150) / 1000m
        };

        return Task.FromResult<Coordenada?>(coordenada);
    }
}