using System.ComponentModel.DataAnnotations;

namespace RuteaYa.Web.Models
{
    public class RegistroViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100)]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [MaxLength(100)]
        public string Apellido { get; set; } = null!;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Confirmá la contraseña")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmarPassword { get; set; } = null!;
    }
}
