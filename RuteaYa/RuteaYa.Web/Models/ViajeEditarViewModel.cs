using System.ComponentModel.DataAnnotations;

namespace RuteaYa.Web.Models
{
    public class ViajeEditarViewModel : IValidatableObject
    {
        public int Id { get; set; } // Fundamental: La "patente" del viaje a editar

        [Required(ErrorMessage = "El origen es obligatorio.")]
        [StringLength(250, ErrorMessage = "El origen puede tener hasta 250 caracteres.")]
        [Display(Name = "Origen")]
        public string OrigenDir { get; set; } = null!;

        [Required(ErrorMessage = "El destino es obligatorio.")]
        [StringLength(250, ErrorMessage = "El destino puede tener hasta 250 caracteres.")]
        [Display(Name = "Destino")]
        public string DestinoDir { get; set; } = null!;

        [Display(Name = "Paradas intermedias")]
        public List<string> ParadasIntermedias { get; set; } = new List<string>();

        // MinLength hace que se valide junto con los demás campos (aunque falte el origen o el destino)
        [Required(ErrorMessage = "Elegí al menos un día de la semana.")]
        [MinLength(1, ErrorMessage = "Elegí al menos un día de la semana.")]
        [Display(Name = "Días de la semana")]
        public List<int> DiasSeleccionados { get; set; } = new List<int>();

        [Required(ErrorMessage = "Ingresá la hora de salida.")]
        [Display(Name = "Hora de salida")]
        public TimeOnly? HoraSalida { get; set; }

        [Required(ErrorMessage = "Ingresá la hora de llegada.")]
        [Display(Name = "Hora de llegada")]
        public TimeOnly? HoraLlegada { get; set; }

        [Required(ErrorMessage = "Ingresá el máximo de paquetes.")]
        [Range(1, 100, ErrorMessage = "El máximo de paquetes tiene que estar entre 1 y 100.")]
        [Display(Name = "Máximo de paquetes")]
        public int MaxPaquetes { get; set; } = 1;

        [Required(ErrorMessage = "Ingresá el peso máximo.")]
        [Range(0.1, 1000, ErrorMessage = "El peso máximo tiene que estar entre 0,1 y 1.000 kg.")]
        [Display(Name = "Peso máximo (kg)")]
        public decimal PesoMaxKg { get; set; } = 1;

        [Required(ErrorMessage = "Ingresá el desvío máximo.")]
        [Range(0, 100, ErrorMessage = "El desvío máximo tiene que estar entre 0 y 100 km.")]
        [Display(Name = "Desvío máximo (km)")]
        public decimal DesvioMaxKm { get; set; } = 0;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (HoraSalida != null && HoraLlegada != null && HoraLlegada <= HoraSalida)
            {
                yield return new ValidationResult(
                    "La hora de llegada debe ser posterior a la hora de salida.",
                    new[] { nameof(HoraLlegada) });
            }

            if (!string.IsNullOrEmpty(OrigenDir) && !string.IsNullOrEmpty(DestinoDir) &&
                OrigenDir.Trim().Equals(DestinoDir.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult("El origen y el destino no pueden ser iguales.", new[] { nameof(DestinoDir) });
            }

            if (ParadasIntermedias != null && ParadasIntermedias.Any(p => p != null && p.Length > 250))
            {
                yield return new ValidationResult("Cada parada puede tener hasta 250 caracteres.", new[] { nameof(ParadasIntermedias) });
            }
        }
    }
}