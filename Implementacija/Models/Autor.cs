using System.ComponentModel.DataAnnotations;

namespace bibliotecha.Models
{
    public class Autor
    {
        [Key]
        public int IdAutora { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }

        public Autor() { }
    }
}
