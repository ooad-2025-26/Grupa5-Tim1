using System.ComponentModel.DataAnnotations;
using bibliotecha.Models;

namespace bibliotecha.Models.ViewModels
{
    public class KreirajKorisnikaViewModel
    {
        [Required(ErrorMessage = "Ime je obavezno.")]
        [StringLength(50, MinimumLength = 2)]
        public string Ime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [StringLength(50, MinimumLength = 2)]
        public string Prezime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Email adresa nije ispravna.")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Lozinka mora imati najmanje 6 karaktera.")]
        public string? Lozinka { get; set; }

        [Required(ErrorMessage = "Uloga je obavezna.")]
        [Range((int)Uloga.Bibliotekar, (int)Uloga.Administrator, ErrorMessage = "Moguće je kreirati samo bibliotekarski ili administratorski nalog.")]
        public Uloga Uloga { get; set; }

   
    }
}