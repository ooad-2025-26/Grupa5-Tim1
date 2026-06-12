using System.ComponentModel.DataAnnotations;
using bibliotecha.Models;

namespace bibliotecha.Models.ViewModels
{
    public class UrediKorisnikaViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ime je obavezno.")]
        [StringLength(50, MinimumLength = 2)]
        public string Ime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [StringLength(50, MinimumLength = 2)]
        public string Prezime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Email adresa nije ispravna.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Uloga je obavezna.")]
        [Range(
    (int)Uloga.Bibliotekar,
    (int)Uloga.Administrator,
    ErrorMessage = "Moguće je odabrati samo bibliotekarsku ili administratorsku ulogu.")]
        public Uloga Uloga { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? DatumZaposlenja { get; set; }
    }
}