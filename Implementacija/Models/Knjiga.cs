using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bibliotecha.Models
{
    public class Knjiga
    {
        [Key] 
        public int IdKnjige { get; set; }

        public string ISBN { get; set; }

        public string Naslov { get; set; }

        [ForeignKey("Autor")]
        public int AutorId { get; set; }
        public Autor Autor { get; set; }

        public Zanr Zanr { get; set; }

        public string Opis { get; set; }
        
        public DateOnly DatumIzdavanja { get; set; }

        public string Izdavac { get; set; }

        public int BrojStranica { get; set; }

        public Jezik Jezik { get; set; }

        public string KoricaKnjige { get; set; }

        public float ProsjecnaOcjena { get; set; }

        public Knjiga() { }
    }
}
