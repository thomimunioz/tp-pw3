using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Logica.Account;
using RuteaYa.Logica.Mapas;
using RuteaYa.Logica.Envios;

using RuteaYa.Logica.Viajes;

using RuteaYa.Logica.Oportunidades;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    var politica = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.Filters.Add(new AuthorizeFilter(politica));
});
builder.Services.AddDbContext<RuteaYaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RuteaYa")));

builder.Services.AddIdentity<Usuario, IdentityRole<int>>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<RuteaYaContext>();

builder.Services.AddScoped<IAccountServicios, AccountServicios>();
builder.Services.AddScoped<IEnvioServicios, EnvioServicios>();

builder.Services.AddScoped<IViajeServicios, ViajeServicios>();

builder.Services.AddScoped<IOportunidadesServicios, OportunidadesServicios>();


if (builder.Configuration.GetValue<bool>("Mapas:UsarGeocodificadorFalso"))
{
    builder.Services.AddScoped<IGeocodificador, GeocodificadorFalso>();
}
else
{
    builder.Services.AddHttpClient<IGeocodificador, NominatimGeocodificador>(client =>
    {
        client.DefaultRequestHeaders.UserAgent.ParseAdd("RuteaYa/1.0 (TP Programacion Web III - UNLaM)");
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/NoEncontrado");
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
