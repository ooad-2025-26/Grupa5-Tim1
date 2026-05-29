using System.ComponentModel.DataAnnotations;

namespace bibliotecha.Models
{
    public enum VrstaObavjestenja
    {
        [Display(Name = "Ističe rok")]
        isticeRok,

        [Display(Name = "Knjiga dostupna")]
        knjigaDostupna
    }
}