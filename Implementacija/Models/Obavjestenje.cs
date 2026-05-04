using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bibliotecha.Models
{
    public class Obavjestenje
    {
        [Key]
        public int IdObavjestenja {  get; set; }


        [ForeignKey("Korisnik")]
        public int KorisnikId { get; set; }
        public Korisnik Korisnik { get; set; }

        public string Poruka { get; set; }

        public DateOnly DatumSlanja { get; set; }

        public VrstaObavjestenja VrstaObavjestenja { get; set; }

        public Obavjestenje() { }

    }
}
