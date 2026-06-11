using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bibliotecha.Models
{
    public class Rezervacija
    {
        [Key]
        [DisplayName("ID rezervacije")]
        public int IdRezervacije { get; set; }

        [ForeignKey("Knjiga")]
        [Required(ErrorMessage = "Knjiga je obavezna.")]
        [DisplayName("Knjiga")]
        public int KnjigaId { get; set; }
        public Knjiga Knjiga { get; set; } = null!;

        [ForeignKey("Korisnik")]
        [Required(ErrorMessage = "Korisnik je obavezan.")]
        [DisplayName("Korisnik")]
        public string KorisnikId { get; set; }
        public Korisnik Korisnik { get; set; } = null!;

        [Required(ErrorMessage = "Datum rezervacije je obavezan.")]
        [DataType(DataType.Date)]
        [DisplayName("Datum rezervacije")]
        public DateOnly DatumRezervacije { get; set; }

        [Required(ErrorMessage = "Status rezervacije je obavezan.")]
        [EnumDataType(typeof(StatusRezervacije))]
        [DisplayName("Status")]
        public StatusRezervacije Status { get; set; }

        [Required(ErrorMessage = "Pozicija u redu je obavezna.")]
        [Range(1, 1000, ErrorMessage = "Pozicija u redu mora biti veća od 0.")]
        [DisplayName("Pozicija u redu")]
        public int PozicijaURedu { get; set; }

        public Rezervacija() { }
    }
}