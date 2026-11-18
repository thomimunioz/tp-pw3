using System.ComponentModel.DataAnnotations;

namespace RuteaYa.Api.Dtos;

public class RegistroRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100)]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [MaxLength(100)]
    public string Apellido { get; set; } = null!;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Tenés que confirmar la contraseña.")]
    [Compare(nameof(Password), ErrorMessage = "La confirmación no coincide con la contraseña.")]
    public string ConfirmarPassword { get; set; } = null!;
}

public class LoginRequest
{
    [Required(ErrorMessage = "El email es obligatorio.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = null!;
}

public record AuthRespuestaDto(
    string Token, DateTime ExpiraEn, int UsuarioId, string Nombre, string Apellido, string Email);

public record UsuarioActualDto(int Id, string Nombre);