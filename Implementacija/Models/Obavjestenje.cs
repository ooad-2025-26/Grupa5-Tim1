using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bibliotecha.Models
{
    public class Obavjestenje
    {
        [Key]
        [DisplayName("ID obavještenja")]
        public int IdObavjestenja {  get; set; }

        [ForeignKey("Korisnik")]
        [Required(ErrorMessage = "Korisnik je obavezan.")]
        [DisplayName("Korisnik")]
        public string KorisnikId { get; set; } = string.Empty;
        public Korisnik Korisnik { get; set; } = null!;

        [Required(ErrorMessage = "Poruka je obavezna.")]
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "Poruka mora imati između 5 i 1000 karaktera.")]
        [DisplayName("Poruka")]
        public string Poruka { get; set; }

        [Required(ErrorMessage = "Datum slanja je obavezan.")]
        [DataType(DataType.Date)]
        [DisplayName("Datum slanja")]
        public DateOnly DatumSlanja { get; set; }

        [Required(ErrorMessage = "Vrsta obavještenja je obavezna.")]
        [EnumDataType(typeof(VrstaObavjestenja))]
        [DisplayName("Vrsta obavještenja")]
        public VrstaObavjestenja VrstaObavjestenja { get; set; }

        public Obavjestenje() { }

    }
}
