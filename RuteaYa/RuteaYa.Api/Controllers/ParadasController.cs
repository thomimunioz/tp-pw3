using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RuteaYa.Api.Dtos;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;

namespace RuteaYa.Api.Controllers;

[ApiController]
[Route("api/paradas")]
public class ParadasController : ControllerBase
{
    private readonly RuteaYaContext _context;

    public ParadasController(RuteaYaContext context)
    {
        _context = context;
    }

    // GET: api/paradas/viaje/1 (Trae todas las paradas ordenadas de un viaje)
    [HttpGet("viaje/{viajeId:int}")]
    [ProducesResponseType(typeof(List<ParadaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ParadaDto>>> GetPorViaje(int viajeId)
    {
        var existeViaje = await _context.Set<Viaje>().AnyAsync(v => v.Id == viajeId);
        if (!existeViaje)
            return NotFound(new { mensaje = $"No existe un viaje con ID {viajeId}." });

        var paradas = await _context.Set<Parada>()
            .AsNoTracking()
            .Where(p => p.ViajeId == viajeId)
            .OrderBy(p => p.Orden)
            .Select(p => new ParadaDto
            {
                Id = p.Id,
                ViajeId = p.ViajeId,
                Orden = p.Orden,
                Direccion = p.Direccion,
                Lat = p.Lat,
                Lng = p.Lng
            })
            .ToListAsync();

        return Ok(paradas);
    }

    // GET: api/paradas/5 (Trae una parada puntual por su ID)
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ParadaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParadaDto>> GetById(int id)
    {
        var parada = await _context.Set<Parada>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (parada == null)
            return NotFound(new { mensaje = $"No se encontró la parada con ID {id}." });

        var dto = new ParadaDto
        {
            Id = parada.Id,
            ViajeId = parada.ViajeId,
            Orden = parada.Orden,
            Direccion = parada.Direccion,
            Lat = parada.Lat,
            Lng = parada.Lng
        };

        return Ok(dto);
    }

    // POST: api/paradas (Crea una parada nueva y devuelve 201 Created)
    [HttpPost]
    [ProducesResponseType(typeof(ParadaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParadaDto>> Crear([FromBody] CrearParadaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Direccion))
            return BadRequest(new { mensaje = "La dirección es obligatoria." });

        var existeViaje = await _context.Set<Viaje>().AnyAsync(v => v.Id == dto.ViajeId);
        if (!existeViaje)
            return NotFound(new { mensaje = $"No existe el viaje con ID {dto.ViajeId}." });

        var nuevaParada = new Parada
        {
            ViajeId = dto.ViajeId,
            Orden = dto.Orden,
            Direccion = dto.Direccion.Trim(),
            Lat = dto.Lat,
            Lng = dto.Lng
        };

        _context.Set<Parada>().Add(nuevaParada);
        await _context.SaveChangesAsync();

        var resultado = new ParadaDto
        {
            Id = nuevaParada.Id,
            ViajeId = nuevaParada.ViajeId,
            Orden = nuevaParada.Orden,
            Direccion = nuevaParada.Direccion,
            Lat = nuevaParada.Lat,
            Lng = nuevaParada.Lng
        };

        return CreatedAtAction(nameof(GetById), new { id = nuevaParada.Id }, resultado);
    }

    // DELETE: api/paradas/5 (Elimina una parada y devuelve 204 No Content)
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var parada = await _context.Set<Parada>().FirstOrDefaultAsync(p => p.Id == id);
        if (parada == null)
            return NotFound(new { mensaje = $"No se encontró la parada con ID {id}." });

        _context.Set<Parada>().Remove(parada);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}