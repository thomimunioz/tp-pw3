using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades.Data;

namespace RuteaYa.Tests;

public static class ContextoDePrueba
{
    public static RuteaYaContext Crear()
    {
        var opciones = new DbContextOptionsBuilder<RuteaYaContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RuteaYaContext(opciones);
    }
}