using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bibliotecha.Models
{
    public class Posudba
    {
        [Key]
        [DisplayName("ID posudbe")]
        public int IdPosudbe { get; set; }

        [ForeignKey("Primjerak")]
        [Required(ErrorMessage = "Primjerak knjige je obavezan.")]
        [DisplayName("Primjerak")]
        public int PrimjerakId { get; set; }
        public Primjerak Primjerak { get; set; } = null!;

        [ForeignKey("Korisnik")]
        [Required(ErrorMessage = "Korisnik je obavezan.")]
        [DisplayName("Korisnik")]
        public string KorisnikId { get; set; } = string.Empty;
        public Korisnik Korisnik { get; set; } = null!;

        [DataType(DataType.Date)]
        [DisplayName("Datum online posudbe")]
        public DateOnly? DatumOnlinePosudbe { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Datum preuzimanja")]
        public DateOnly? DatumPreuzimanja { get; set; }

        [Required(ErrorMessage = "Rok vraćanja je obavezan.")]
        [DataType(DataType.Date)]
        [DisplayName("Rok vraćanja")]
        public DateOnly RokVracanja { get; set; }

        [Required(ErrorMessage = "Status posudbe je obavezan.")]
        [EnumDataType(typeof(StatusPosudbe))]
        [DisplayName("Status")]
        public StatusPosudbe Status {  get; set; }

        [Range(0, 3, ErrorMessage = "Broj produženja mora biti između 0 i 3.")]
        [DisplayName("Broj produženja")]
        public int? BrojProduzenja { get; set; }

        public Posudba() { }

    }
}
