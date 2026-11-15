using System.ComponentModel.DataAnnotations;

namespace RuteaYa.Api.Dtos;

public class ConfirmarCombinacionRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "Tenés que mandar al menos un envío.")]
    public List<int> EnvioIds { get; set; } = new();
}
