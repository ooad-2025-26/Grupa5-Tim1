using bibliotecha.Models;

namespace bibliotecha.Models.ViewModels
{
    public class KorisnikListaViewModel
    {
        public List<Korisnik> Korisnici { get; set; } = new();
        public string? Upit { get; set; }
        public Uloga? Uloga { get; set; }
        public bool? Aktivan { get; set; }
    }
}
