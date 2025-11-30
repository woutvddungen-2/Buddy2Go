using Shared.Models.enums;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Dtos.DangerousPlaces
{
    public class DangerousPlaceCreateDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Je dient een categorie te kiezen")]
        public DangerousPlaceType PlaceType { get; set; } = DangerousPlaceType.Other;

        public string? Description { get; set; }

        [Required(ErrorMessage = "GPS dient ingevuld te worden")]
        public string GPS { get; set; } = string.Empty;
    }
}
