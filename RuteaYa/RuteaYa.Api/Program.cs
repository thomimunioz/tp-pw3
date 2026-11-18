using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades.Data;
using RuteaYa.Logica.Asignaciones;
using RuteaYa.Logica.Oportunidades;
using RuteaYa.Logica.Optimizacion;
using RuteaYa.Logica.Viajes;
using RuteaYa.Logica.Transportes;
using RuteaYa.Logica.Historial;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<RuteaYaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RuteaYa")));

builder.Services.AddScoped<IViajeListadoServicios, ViajeListadoServicios>();
builder.Services.AddScoped<IOportunidadesServicios, OportunidadesServicios>();
builder.Services.AddScoped<IAsignacionesServicios, AsignacionesServicios>();
builder.Services.AddScoped<IOptimizadorServicios, OptimizadorServicios>();
builder.Services.AddScoped<ITransporteServicios, TransporteServicios>();
builder.Services.AddScoped<IHistorialServicios, HistorialServicios>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();