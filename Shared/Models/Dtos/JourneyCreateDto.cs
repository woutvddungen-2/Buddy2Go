using Shared.Validation;
using System.ComponentModel.DataAnnotations;

public class JourneyCreateDto
{
    [Required(ErrorMessage = "Startplaats is verplicht.")]
    [Range(1, int.MaxValue, ErrorMessage = "Startplaats is verplicht.")]
    public int StartPlaceId { get; set; }

    [Required(ErrorMessage = "Eindplaats is verplicht.")]
    [Range(1, int.MaxValue, ErrorMessage = "Eindplaats is verplicht.")]
    public int EndPlaceId { get; set; }

    [Required(ErrorMessage = "Starttijd is verplicht.")]
    [NotPastDate]
    public DateTime StartAt { get; set; }
}
