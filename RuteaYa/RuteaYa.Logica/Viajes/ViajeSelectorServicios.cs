using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades.Data;

namespace RuteaYa.Logica.Viajes;

public class ViajeOpcionDto
{
    public int Id { get; set; }
    public string OrigenDir { get; set; } = "";
    public string DestinoDir { get; set; } = "";
    public TimeOnly HoraSalida { get; set; }
    public List<DayOfWeek> Dias { get; set; } = new();
}

public interface IViajeSelectorServicios
{
    Task<List<ViajeOpcionDto>> ObtenerActivosAsync(int usuarioId);
    Task<int?> ObtenerViajeIdPorDefectoAsync(int usuarioId, DateTime ahora);
}

public class ViajeSelectorServicios : IViajeSelectorServicios
{
    private readonly RuteaYaContext _context;

    public ViajeSelectorServicios(RuteaYaContext context)
    {
        _context = context;
    }

    public async Task<List<ViajeOpcionDto>> ObtenerActivosAsync(int usuarioId)
    {
        return await _context.Viajes
            .AsNoTracking()
            .Where(v => v.UsuarioId == usuarioId && v.Activo)
            .OrderBy(v => v.Id)
            .Select(v => new ViajeOpcionDto
            {
                Id = v.Id,
                OrigenDir = v.OrigenDir,
                DestinoDir = v.DestinoDir,
                HoraSalida = v.HoraSalida,
                Dias = v.Dias.Select(d => d.DiaSemana).ToList()
            })
            .ToListAsync();
    }

    public async Task<int?> ObtenerViajeIdPorDefectoAsync(int usuarioId, DateTime ahora)
    {
        var activos = await ObtenerActivosAsync(usuarioId);

        var proximo = activos
            .Select(v => new { v.Id, Salida = ProximaSalida(v, ahora) })
            .Where(x => x.Salida != null)
            .OrderBy(x => x.Salida)
            .ThenBy(x => x.Id)
            .FirstOrDefault();

        // Red de seguridad: si ningún viaje tuviera días cargados, devuelve el primero activo
        return proximo?.Id ?? activos.FirstOrDefault()?.Id;
    }

    private static DateTime? ProximaSalida(ViajeOpcionDto viaje, DateTime ahora)
    {
        if (viaje.Dias.Count == 0) return null;

        return viaje.Dias
            .Select(dia =>
            {
                var diasHasta = ((int)dia - (int)ahora.DayOfWeek + 7) % 7;
                var salida = ahora.Date.AddDays(diasHasta).Add(viaje.HoraSalida.ToTimeSpan());

                // Si esa salida ya pasó (hoy, pero más temprano), la próxima es en 7 días
                return salida < ahora ? salida.AddDays(7) : salida;
            })
            .Min();
    }
}