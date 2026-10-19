using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace RuteaYa.Entidades.Data
{
    public class RuteaYaContext : IdentityDbContext<Usuario, IdentityRole<int>, int>
    {

        public RuteaYaContext(DbContextOptions<RuteaYaContext> options) : base(options)
        {
        }

        public DbSet<Viaje> Viajes { get; set; }
        public DbSet<Parada> Paradas { get; set; }
        public DbSet<ViajeDia> ViajeDias { get; set; }
        public DbSet<Envio> Envios { get; set; }
        public DbSet<Asignacion> Asignaciones { get; set; }
        public DbSet<EnvioEstadoHistorial> EnvioEstadoHistoriales { get; set; }
    }
}
