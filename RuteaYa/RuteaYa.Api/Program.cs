using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades.Data;
using RuteaYa.Logica.Viajes;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<RuteaYaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RuteaYa")));

builder.Services.AddScoped<IViajeListadoServicios, ViajeListadoServicios>();

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