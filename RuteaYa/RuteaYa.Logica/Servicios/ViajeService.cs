using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;

namespace RuteaYa.Logica.Viajes;

public class ViajeServicios : IViajeServicios
{
    private readonly RuteaYaContext _context;

    public ViajeServicios(RuteaYaContext context)
    {
        _context = context;
    }

    public async Task<List<Viaje>> ObtenerPorUsuarioAsync(int usuarioId)
    {
        return await _context.Viajes
            .AsNoTracking()
            .Where(v => v.UsuarioId == usuarioId)
            .OrderByDescending(v => v.Id)
            .ToListAsync();
    }

    public async Task CrearViajeAsync(Viaje viaje, List<int> diasSeleccionados, string? paradasIntermedias)
    {
        ArgumentNullException.ThrowIfNull(viaje);

        viaje.Activo = true;

        var diasUnicos = diasSeleccionados
            .Where(d => Enum.IsDefined(typeof(DayOfWeek), d))
            .Distinct()
            .ToList();

        foreach (var dia in diasUnicos)
        {
            viaje.Dias.Add(new ViajeDia
            {
                Viaje = viaje,
                DiaSemana = (DayOfWeek)dia
            });
        }

        if (!string.IsNullOrWhiteSpace(paradasIntermedias))
        {
            var origenNormalizado = viaje.OrigenDir?.Trim();
            var destinoNormalizado = viaje.DestinoDir?.Trim();

            var paradas = paradasIntermedias
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(p =>
                    !string.Equals(p, origenNormalizado, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(p, destinoNormalizado, StringComparison.OrdinalIgnoreCase))
                .Select((direccion, indice) => new Parada
                {
                    Viaje = viaje,
                    Orden = indice + 1,
                    Direccion = direccion,
                    Lat = 0m,
                    Lng = 0m
                })
                .ToList();

            foreach (var parada in paradas)
            {
                viaje.Paradas.Add(parada);
            }
        }

        _context.Viajes.Add(viaje);
        await _context.SaveChangesAsync();
    }
}



