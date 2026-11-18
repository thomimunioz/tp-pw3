using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RuteaYa.Api.Auth;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Logica.Account;
using RuteaYa.Logica.Asignaciones;
using RuteaYa.Logica.Oportunidades;
using RuteaYa.Logica.Optimizacion;
using RuteaYa.Logica.Viajes;
using System.Text;
using RuteaYa.Logica.Transportes;
using RuteaYa.Logica.Historial;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<RuteaYaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RuteaYa")));

// Identity sin cookies: la API solo usa UserManager para validar y crear usuarios
builder.Services.AddIdentityCore<Usuario>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<RuteaYaContext>();

// JWT
var seccionJwt = builder.Configuration.GetSection(JwtOpciones.Seccion);
builder.Services.Configure<JwtOpciones>(seccionJwt);
var jwt = seccionJwt.Get<JwtOpciones>()
    ?? throw new InvalidOperationException("Falta la sección 'Jwt' en la configuración.");
if (string.IsNullOrWhiteSpace(jwt.Clave) || jwt.Clave.Length < 32)
    throw new InvalidOperationException("Jwt:Clave tiene que tener al menos 32 caracteres.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Emisor,
            ValidateAudience = true,
            ValidAudience = jwt.Audiencia,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        // Si queremos que los tokens dejen de ser válidos cuando un usuario es eliminado.
        //options.Events = new JwtBearerEvents
        //{
        //    OnTokenValidated = async contexto =>
        //    {
        //        var userManager = contexto.HttpContext.RequestServices
        //            .GetRequiredService<UserManager<Usuario>>();
        //        var id = contexto.Principal!.ObtenerUsuarioId();

        //        if (await userManager.FindByIdAsync(id.ToString()) == null)
        //            contexto.Fail("El usuario ya no existe.");
        //    }
        //};
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<IAccountServicios, AccountServicios>();
builder.Services.AddScoped<IGeneradorTokens, GeneradorTokens>();

builder.Services.AddScoped<IViajeListadoServicios, ViajeListadoServicios>();
builder.Services.AddScoped<IOportunidadesServicios, OportunidadesServicios>();
builder.Services.AddScoped<IAsignacionesServicios, AsignacionesServicios>();
builder.Services.AddScoped<IOptimizadorServicios, OptimizadorServicios>();
builder.Services.AddScoped<ITransporteServicios, TransporteServicios>();
builder.Services.AddScoped<IHistorialServicios, HistorialServicios>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Botón "Authorize" en Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Pegá el token que devuelve POST /api/autenticacion/ingresar (sin la palabra Bearer)."
    });
    options.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", documento)] = []
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();