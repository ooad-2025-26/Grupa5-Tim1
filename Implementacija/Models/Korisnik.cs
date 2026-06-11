using Microsoft.AspNetCore.Identity;using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace bibliotecha.Models
{
    public class Korisnik : IdentityUser
    {
        [Range(1, int.MaxValue, ErrorMessage = "Broj članske kartice mora biti pozitivan broj.")]
        [DisplayName("Broj članske kartice")]
        public int? BrojClanskeKartice { get; set; }
        [Required(ErrorMessage = "Ime je obavezno.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ime mora imati između 2 i 50 karaktera.")]
        [RegularExpression(@"^[A-ZČĆŽŠĐa-zčćžšđ\s'-]+$", ErrorMessage = "Ime smije sadržavati samo slova, razmake, apostrof i crticu.")]
        [DisplayName("Ime")]
        public string Ime { get; set; } = string.Empty;
        [Required(ErrorMessage = "Prezime je obavezno.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Prezime mora imati između 2 i 50 karaktera.")]
        [RegularExpression(@"^[A-ZČĆŽŠĐa-zčćžšđ\s'-]+$", ErrorMessage = "Prezime smije sadržavati samo slova, razmake, apostrof i crticu.")]
        [DisplayName("Prezime")]
        public string Prezime { get; set; } = string.Empty;
        [DataType(DataType.Date)]
        [DisplayName("Datum zaposlenja")]
        public DateOnly? DatumZaposlenja { get; set; }
        [DataType(DataType.Date)]
        [DisplayName("Datum registracije")]
        public DateOnly? DatumRegistracije { get; set; }
        [Required(ErrorMessage = "Uloga korisnika je obavezna.")]
        [EnumDataType(typeof(Uloga))]
        [DisplayName("Uloga")]
        public Uloga Uloga { get; set; }

        public Korisnik() { }

    }
}