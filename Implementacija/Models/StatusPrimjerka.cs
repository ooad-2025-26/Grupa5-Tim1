using System.ComponentModel.DataAnnotations;

namespace bibliotecha.Models
{
    public enum StatusPrimjerka
    {
        [Display(Name = "Dostupan")]
        Dostupan,

        [Display(Name = "Posuđen")]
        Posudjen,

        [Display(Name = "Rezervisan")]
        Rezervisan,

        [Display(Name = "Izgubljen")]
        Izgubljen
    }
}