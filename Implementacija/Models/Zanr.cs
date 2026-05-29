using System.ComponentModel.DataAnnotations;

namespace bibliotecha.Models
{
    public enum Zanr
    {
        [Display(Name = "Klasik")]
        Klasik,

        [Display(Name = "Fantazija")]
        Fantazija,

        [Display(Name = "Triler")]
        Triler,

        [Display(Name = "Misterija")]
        Misterija,

        [Display(Name = "Drama")]
        Drama,

        [Display(Name = "Poezija")]
        Poezija,

        [Display(Name = "Horor")]
        Horor,

        [Display(Name = "Biografija")]
        Biografija,

        [Display(Name = "Historija")]
        Historija,

        [Display(Name = "Naučne knjige")]
        Naucne,

        [Display(Name = "Dječije knjige")]
        Djecije,

        [Display(Name = "Ostalo")]
        Ostalo
    }
}