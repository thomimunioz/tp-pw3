using System.ComponentModel.DataAnnotations;

namespace RuteaYa.Web.ViewModels;

public class ViajeCrearViewModel : IValidatableObject
{
    [Required(ErrorMessage = "El origen es obligatorio.")]
    [Display(Name = "Origen")]
    public string OrigenDir { get; set; } = string.Empty;

    [Required(ErrorMessage = "El destino es obligatorio.")]
    [Display(Name = "Destino")]
    public string DestinoDir { get; set; } = string.Empty;

    [Display(Name = "Paradas intermedias")]
    public string? ParadasIntermedias { get; set; }

    [Required(ErrorMessage = "Debes seleccionar al menos un día.")]
    [Display(Name = "Días de la semana")]
    public List<int> DiasSeleccionados { get; set; } = new();

    [Display(Name = "Hora de salida")]
    [DataType(DataType.Time)]
    public TimeOnly HoraSalida { get; set; }

    [Display(Name = "Hora de llegada")]
    [DataType(DataType.Time)]
    public TimeOnly HoraLlegada { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe haber al menos 1 paquete.")]
    [Display(Name = "Máximo de paquetes")]
    public int MaxPaquetes { get; set; } = 1;

    [Range(0.1, double.MaxValue, ErrorMessage = "El peso máximo debe ser mayor a 0.")]
    [Display(Name = "Peso máximo (kg)")]
    public decimal PesoMaxKg { get; set; } = 0.1m;

    [Range(0, double.MaxValue, ErrorMessage = "El desvío máximo no puede ser negativo.")]
    [Display(Name = "Desvío máximo (km)")]
    public decimal DesvioMaxKm { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(OrigenDir) &&
            !string.IsNullOrWhiteSpace(DestinoDir) &&
            string.Equals(OrigenDir.Trim(), DestinoDir.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "El origen y el destino deben ser distintos.",
                new[] { nameof(OrigenDir), nameof(DestinoDir) });
        }

        if (DiasSeleccionados == null || DiasSeleccionados.Count == 0)
        {
            yield return new ValidationResult(
                "Debes seleccionar al menos un día para el viaje.",
                new[] { nameof(DiasSeleccionados) });
        }
    }
}
