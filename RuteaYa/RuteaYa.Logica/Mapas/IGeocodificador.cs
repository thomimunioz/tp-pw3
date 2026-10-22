namespace RuteaYa.Logica.Mapas;

public interface IGeocodificador
{
    Task<Coordenada?> GeocodificarAsync(string direccion);
}


