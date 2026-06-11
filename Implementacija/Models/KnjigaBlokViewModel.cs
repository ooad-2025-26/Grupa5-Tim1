namespace bibliotecha.Models
{
    public class KnjigaBlokViewModel
    {
        public Zanr Zanr { get; set; }
        public string NazivZanra { get; set; } = string.Empty;
        public List<Knjiga> Knjige { get; set; } = new();
    }
}