using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace bibliotecha.Models
{
    public class Korisnik: IdentityUser
    {
        public int? BrojClanskeKartice { get; set; }

        public string Ime { get; set; } = string.Empty;

        public string Prezime { get; set; } = string.Empty;

        public DateOnly? DatumZaposlenja { get; set; }

        public DateOnly? DatumRegistracije { get; set; }

        public Uloga Uloga { get; set; }

        public Korisnik() { }

    }
}
