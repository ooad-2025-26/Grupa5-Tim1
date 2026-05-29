using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bibliotecha.Models
{
    public class Primjerak
    {
        [Key]
        [DisplayName("ID primjerka")]
        public int IdPrimjerka { get; set; }

        [Required(ErrorMessage = "Status primjerka je obavezan.")]
        [EnumDataType(typeof(StatusPrimjerka))]
        [DisplayName("Status")]
        public StatusPrimjerka Status { get; set; }

        [ForeignKey("Knjiga")]
        [Required(ErrorMessage = "Knjiga je obavezna.")]
        [DisplayName("Knjiga")]
        public int KnjigaId { get; set; }
        public Knjiga Knjiga { get; set; } = null!;

        public Primjerak() { }
    }
}
