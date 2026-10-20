using System.ComponentModel.DataAnnotations;
namespace RuteaYa.Web.Models;


public class LoginViewModel
{
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El email no es válido")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Display(Name = "Recordarme")]
    public bool Recordarme { get; set; }
}
