using System.ComponentModel.DataAnnotations;

public enum DangerousPlaceType
{
    [Display(Name = "Gevaarlijke weg/kruising")]
    AccidentProne = 0,

    [Display(Name = "Criminaliteit")]
    CrimeSpot = 1,

    [Display(Name = "Vuilnis / zwerfafval")]
    Trash = 2,

    [Display(Name = "Slechte verlichting")]
    PoorLighting = 3,

    [Display(Name = "Overig")]
    Other = 4
}