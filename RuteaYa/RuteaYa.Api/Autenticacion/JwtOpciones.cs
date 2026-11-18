namespace RuteaYa.Api.Autenticacion;

public class JwtOpciones
{
    public const string Seccion = "Jwt";

    public string Clave { get; set; } = null!;
    public string Emisor { get; set; } = null!;
    public string Audiencia { get; set; } = null!;
    public int MinutosExpiracion { get; set; } = 60;
}