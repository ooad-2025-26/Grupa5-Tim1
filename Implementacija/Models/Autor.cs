using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace bibliotecha.Models
{
    public class Autor
    {
        [Key]
        [DisplayName("ID autora")]
        public int IdAutora { get; set; }

        [Required(ErrorMessage = "Ime autora je obavezno.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ime autora mora imati između 2 i 50 karaktera.")]
        [RegularExpression(@"^[A-ZČĆŽŠĐa-zčćžšđ\s'-]+$", ErrorMessage = "Ime autora smije sadržavati samo slova, razmake, apostrof i crticu.")]
        [DisplayName("Ime")]
        public string Ime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime autora je obavezno.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Prezime autora mora imati između 2 i 50 karaktera.")]
        [RegularExpression(@"^[A-ZČĆŽŠĐa-zčćžšđ\s'-]+$", ErrorMessage = "Prezime autora smije sadržavati samo slova, razmake, apostrof i crticu.")]
        [DisplayName("Prezime")]
        public string Prezime { get; set; } = string.Empty;

        public Autor() { }
    }
}
