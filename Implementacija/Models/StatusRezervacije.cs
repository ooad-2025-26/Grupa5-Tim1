using System.ComponentModel.DataAnnotations;

namespace bibliotecha.Models
{
    public enum StatusRezervacije
    {
        [Display(Name = "Aktivna")]
        Aktivna,

        [Display(Name = "Istekla")]
        Istekla,

        [Display(Name = "Ispunjena")]
        Ispunjena,

        [Display(Name = "Spremna za preuzimanje")]
        spremnaZaPreuzimanje
    }
}