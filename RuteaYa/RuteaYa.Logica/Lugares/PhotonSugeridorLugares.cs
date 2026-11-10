
using Microsoft.Extensions.Logging;
using System.Text.Json;
using static RuteaYa.Logica.Lugares.ISugeridorLugares;

namespace RuteaYa.Logica.Lugares;

public class PhotonSugeridorLugares : ISugeridorLugares
{
    private readonly HttpClient _http;
    private readonly ILogger<PhotonSugeridorLugares> _logger;

    public PhotonSugeridorLugares(
        HttpClient http,
        ILogger<PhotonSugeridorLugares> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<IReadOnlyList<LugarSugeridoDto>> BuscarAsync(
        string texto,
        CancellationToken cancellationToken = default)
    {
        texto = (texto ?? string.Empty).Trim();

        if (texto.Length < LugaresReglas.MinCaracteres)
            return Array.Empty<LugarSugeridoDto>();

        // Buscamos direcciones, calles y localidades.
        // No usamos osm_tag=place porque excluiría otros tipos de resultados.
        var url = $"api/?q={Uri.EscapeDataString(texto)}" +
          $"&limit={LugaresReglas.MaxResultados}&countrycode=AR";

        try
        {
            using var respuesta = await _http.GetAsync(url, cancellationToken);

            respuesta.EnsureSuccessStatusCode();

            await using var contenido =
                await respuesta.Content.ReadAsStreamAsync(cancellationToken);

            using var documento = await JsonDocument.ParseAsync(
                contenido,
                cancellationToken: cancellationToken);

            return ConvertirALugares(documento.RootElement);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // El usuario siguió escribiendo o cerró la petición.
            // Es una cancelación normal del autocompletado.
            return Array.Empty<LugarSugeridoDto>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "No se pudieron obtener sugerencias de lugares desde Photon.");

            return Array.Empty<LugarSugeridoDto>();
        }
    }

    private static IReadOnlyList<LugarSugeridoDto> ConvertirALugares(
        JsonElement raiz)
    {
        if (!raiz.TryGetProperty("features", out var features) ||
            features.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<LugarSugeridoDto>();
        }

        return features.EnumerateArray()
            .Where(f => f.TryGetProperty("properties", out _))
            .Select(f => ArmarNombre(f.GetProperty("properties")))
            .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(nombre => new LugarSugeridoDto(nombre))
            .ToList();
    }

    private static string ArmarNombre(JsonElement propiedades)
    {
        var nombre = LeerTexto(propiedades, "name");
        var calle = LeerTexto(propiedades, "street");
        var numero = LeerTexto(propiedades, "housenumber");

        var partes = new List<string>();

        // Si Photon devuelve una calle y altura, las mostramos juntas.
        if (!string.IsNullOrWhiteSpace(calle))
        {
            partes.Add(
                string.IsNullOrWhiteSpace(numero)
                    ? calle
                    : $"{calle} {numero}");
        }

        // Nombre de localidad, establecimiento u otro lugar.
        if (!string.IsNullOrWhiteSpace(nombre) &&
            !partes.Contains(nombre, StringComparer.OrdinalIgnoreCase))
        {
            partes.Insert(0, nombre);
        }

        // Información geográfica complementaria.
        foreach (var campo in new[]
        {
            "district",
            "city",
            "state",
            "country"
        })
        {
            var valor = LeerTexto(propiedades, campo);

            if (!string.IsNullOrWhiteSpace(valor) &&
                !partes.Contains(valor, StringComparer.OrdinalIgnoreCase))
            {
                partes.Add(valor);
            }
        }

        return string.Join(", ", partes);
    }

    private static string? LeerTexto(
        JsonElement propiedades,
        string campo)
    {
        return propiedades.TryGetProperty(campo, out var valor) &&
               valor.ValueKind == JsonValueKind.String
            ? valor.GetString()
            : null;
    }
}
