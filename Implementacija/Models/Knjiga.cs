using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bibliotecha.Models
{
    public class Knjiga
    {
        [Key]

        [DisplayName("ID knjige")]
        public int IdKnjige { get; set; }

        [Required(ErrorMessage = "ISBN je obavezan.")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "ISBN mora imati između 10 i 20 karaktera.")]
        [RegularExpression(@"^[0-9\-]+$", ErrorMessage = "ISBN smije sadržavati samo brojeve i crtice.")]
        [DisplayName("ISBN")]
        public string ISBN { get; set; } = string.Empty;


        [Required(ErrorMessage = "Naslov knjige je obavezan.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Naslov knjige može imati najviše 100 karaktera.")]
        [DisplayName("Naslov")]
        public string Naslov { get; set; } = string.Empty;

        [ForeignKey("Autor")]
        [Required(ErrorMessage = "Autor je obavezan.")]
        [DisplayName("Autor")]
        public int AutorId { get; set; }
        public Autor Autor { get; set; } = null!;

        [Required(ErrorMessage = "Žanr je obavezan.")]
        [EnumDataType(typeof(Zanr))]
        [DisplayName("Žanr")]
        public Zanr Zanr { get; set; }

        [Required(ErrorMessage = "Opis knjige je obavezan.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Opis knjige mora imati između 10 i 1000 karaktera.")]
        [DisplayName("Opis")]
        public string Opis { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum izdavanja je obavezan.")]
        [DataType(DataType.Date)]
        [DisplayName("Datum izdavanja")]
        public DateOnly DatumIzdavanja { get; set; }

        [Required(ErrorMessage = "Izdavač je obavezan.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Naziv izdavača mora imati između 2 i 100 karaktera.")]
        [DisplayName("Izdavač")]
        public string Izdavac { get; set; } = string.Empty;

        [Required(ErrorMessage = "Broj stranica je obavezan.")]
        [Range(1, 5000, ErrorMessage = "Broj stranica mora biti između 1 i 5000.")]
        [DisplayName("Broj stranica")]
        public int BrojStranica { get; set; }

        [Required(ErrorMessage = "Jezik je obavezan.")]
        [EnumDataType(typeof(Jezik))]
        [DisplayName("Jezik")]
        public Jezik Jezik { get; set; }

        [StringLength(500, ErrorMessage = "Link ili putanja do korice može imati najviše 500 karaktera.")]
        [DisplayName("Korica knjige")]
        public string KoricaKnjige { get; set; } = string.Empty;

        [Range(0, 5, ErrorMessage = "Prosječna ocjena mora biti između 0 i 5.")]
        [DisplayName("Prosječna ocjena")]
        public float ProsjecnaOcjena { get; set; }

        public Knjiga() { }
    }
}