using Shared.Validation;
using System.ComponentModel.DataAnnotations;

public class JourneyCreateDto
{
    [Required(ErrorMessage = "Startplaats is verplicht.")]
    public int StartPlaceId { get; set; }

    [Required(ErrorMessage = "Eindplaats is verplicht.")]
    public int EndPlaceId { get; set; }

    [Required(ErrorMessage = "Starttijd is verplicht.")]
    [NotPastDate]
    public DateTime StartAt { get; set; }
}
