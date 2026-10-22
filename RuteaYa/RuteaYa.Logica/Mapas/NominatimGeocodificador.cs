using System.Globalization;
using System.Net.Http.Json;

namespace RuteaYa.Logica.Mapas;

public class NominatimGeocodificador : IGeocodificador
{
    private readonly HttpClient _http;

    public NominatimGeocodificador(HttpClient http)
    {
        _http = http;
    }

    public async Task<Coordenada?> GeocodificarAsync(string direccion)
    {
        var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(direccion)}&format=json&limit=1&countrycodes=ar";
        var resultados = await _http.GetFromJsonAsync<List<ResultadoNominatim>>(url);
        if (resultados == null || resultados.Count == 0)
        {
            return null;
        }

        var primero = resultados[0];
        decimal lat = decimal.Parse(primero.Lat, CultureInfo.InvariantCulture);
        decimal lon = decimal.Parse(primero.Lon, CultureInfo.InvariantCulture);

        return new Coordenada { Lat = lat, Lng = lon };
    }
}