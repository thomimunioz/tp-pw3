using System.ComponentModel.DataAnnotations;

namespace RuteaYa.Web.Models.Viajes;

public class ViajeCrearViewModel : IValidatableObject
{
    [Required(ErrorMessage = "El origen es obligatorio.")]
    [StringLength(250, ErrorMessage = "El origen puede tener hasta 250 caracteres.")]
    [Display(Name = "Origen")]
    public string OrigenDir { get; set; } = string.Empty;

    [Required(ErrorMessage = "El destino es obligatorio.")]
    [StringLength(250, ErrorMessage = "El destino puede tener hasta 250 caracteres.")]
    [Display(Name = "Destino")]
    public string DestinoDir { get; set; } = string.Empty;

    [Display(Name = "Paradas intermedias")]
    public List<string> ParadasIntermedias { get; set; } = new();

    [Required(ErrorMessage = "Debes seleccionar al menos un día.")]
    [Display(Name = "Días de la semana")]
    public List<int> DiasSeleccionados { get; set; } = new();

    [Display(Name = "Hora de salida")]
    [DataType(DataType.Time)]
    public TimeOnly HoraSalida { get; set; }

    [Display(Name = "Hora de llegada")]
    [DataType(DataType.Time)]
    public TimeOnly HoraLlegada { get; set; }

    [Range(1, 100, ErrorMessage = "El máximo de paquetes tiene que estar entre 1 y 100.")]
    [Display(Name = "Máximo de paquetes")]
    public int MaxPaquetes { get; set; } = 1;

    [Range(0.1, 1000, ErrorMessage = "El peso máximo tiene que estar entre 0,1 y 1.000 kg.")]
    [Display(Name = "Peso máximo (kg)")]
    public decimal PesoMaxKg { get; set; } = 0.1m;

    [Range(0, 100, ErrorMessage = "El desvío máximo tiene que estar entre 0 y 100 km.")]
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

        if (HoraLlegada == HoraSalida)
        {
            yield return new ValidationResult(
                "La hora de llegada no puede ser igual a la de salida.",
                new[] { nameof(HoraLlegada) });
        }

        if (DiasSeleccionados == null || DiasSeleccionados.Count == 0)
        {
            yield return new ValidationResult(
                "Debes seleccionar al menos un día para el viaje.",
                new[] { nameof(DiasSeleccionados) });
        }

        if (ParadasIntermedias != null && ParadasIntermedias.Any(p => p != null && p.Length > 250))
        {
            yield return new ValidationResult(
                "Cada parada puede tener hasta 250 caracteres.",
                new[] { nameof(ParadasIntermedias) });
        }
    }
}
