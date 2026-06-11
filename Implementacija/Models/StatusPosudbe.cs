using System.ComponentModel.DataAnnotations;

namespace bibliotecha.Models
{
    public enum StatusPosudbe
    {
        [Display(Name = "Online")]
        Online,

        [Display(Name = "Aktivna")]
        Aktivna,

        [Display(Name = "Završena")]
        Zavrsena,

        [Display(Name = "Produžena")]
        Produzena,

        [Display(Name = "Kasni")]
        Kasni
    }
}