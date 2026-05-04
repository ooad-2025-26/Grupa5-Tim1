using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bibliotecha.Models
{
    public class Rezervacija
    {
        [Key]
        public int IdRezervacije { get; set; }

        [ForeignKey("Knjiga")]
        public int KnjigaId { get; set; }
        public Knjiga Knjiga { get; set; }

        [ForeignKey("Korisnik")]
        public int KorisnikId { get; set; }
        public Korisnik Korisnik { get; set; }

        public DateOnly DatumRezervacije { get; set; }

        public StatusRezervacije Status { get; set; }

        public int PozicijaURedu { get; set; }

        public Rezervacija() { }
    }
}
