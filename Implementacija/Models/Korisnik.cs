using System.ComponentModel.DataAnnotations;

namespace bibliotecha.Models
{
    public class Korisnik
    {
        [Key]
        public int IdKorisnika {  get; set; }

        public int? BrojClanskeKartice { get; set; }

        public string Ime {  get; set; }

        public string Prezime { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public DateOnly? DatumZaposlenja { get; set; }

        public DateOnly? DatumRegistracije { get; set; }

        public Uloga Uloga { get; set; }

        public Korisnik() { }

    }
}
