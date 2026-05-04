using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bibliotecha.Models
{
    public class Primjerak
    {
        [Key]
        public int IdPrimjerka { get; set; }

        public StatusPrimjerka Status { get; set; }

        [ForeignKey("Knjiga")]
        public int KnjigaId { get; set; }
        public Knjiga Knjiga { get; set; }

        public Primjerak() { }
    }
}
