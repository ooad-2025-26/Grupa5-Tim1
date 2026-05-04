using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bibliotecha.Models
{
    public class Posudba
    {
        [Key]
        public int IdPosudbe { get; set; }

        [ForeignKey("Primjerak")]
        public int PrimjerakId { get; set; }
        public Primjerak Primjerak { get; set; }

        [ForeignKey("Korisnik")]
        public int KorisnikId { get; set; }
        public Korisnik Korisnik { get; set; }

        public DateOnly? DatumOnlinePosudbe { get; set; }

        public DateOnly? DatumPreuzimanja { get; set; }

        public DateOnly RokVracanja { get; set; }

        public StatusPosudbe Status {  get; set; }

        public int? BrojProduzenja { get; set; }

        public Posudba() { }

    }
}
