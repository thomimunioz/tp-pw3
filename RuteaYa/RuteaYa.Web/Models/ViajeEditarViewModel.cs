using System.ComponentModel.DataAnnotations;

namespace RuteaYa.Web.Models
{
    public class ViajeEditarViewModel : IValidatableObject
    {
        public int Id { get; set; } // Fundamental: La "patente" del viaje a editar

        [Required(ErrorMessage = "El origen es obligatorio.")]
        [Display(Name = "Origen")]
        public string OrigenDir { get; set; } = null!;

        [Required(ErrorMessage = "El destino es obligatorio.")]
        [Display(Name = "Destino")]
        public string DestinoDir { get; set; } = null!;

        [Display(Name = "Paradas intermedias")]
        public List<string> ParadasIntermedias { get; set; } = new List<string>();

        [Required(ErrorMessage = "Debes seleccionar al menos un día.")]
        public List<int> DiasSeleccionados { get; set; } = new List<int>();

        [Required(ErrorMessage = "La hora de salida es obligatoria.")]
        public TimeOnly HoraSalida { get; set; }

        [Required(ErrorMessage = "La hora de llegada es obligatoria.")]
        public TimeOnly HoraLlegada { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad máxima de paquetes debe ser mayor a 0.")]
        public int MaxPaquetes { get; set; } = 1;

        [Range(0.1, 9999.99, ErrorMessage = "El peso máximo debe ser mayor a 0.")]
        public decimal PesoMaxKg { get; set; } = 1;

        [Range(0, 9999.99, ErrorMessage = "El desvío máximo debe ser un valor válido.")]
        public decimal DesvioMaxKm { get; set; } = 0;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (HoraLlegada <= HoraSalida)
            {
                yield return new ValidationResult(
                    "La hora de llegada debe ser posterior a la hora de salida.",
                    new[] { nameof(HoraLlegada) });
            }

            if (DiasSeleccionados == null || DiasSeleccionados.Count == 0)
            {
                yield return new ValidationResult(
                    "Debes seleccionar al menos un día para el viaje.",
                    new[] { nameof(DiasSeleccionados) });
            }

            if (!string.IsNullOrEmpty(OrigenDir) && !string.IsNullOrEmpty(DestinoDir) &&
                OrigenDir.Trim().Equals(DestinoDir.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult("El origen y el destino no pueden ser iguales.", new[] { nameof(DestinoDir) });
            }
        }
    }
}