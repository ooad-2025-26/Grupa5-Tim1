namespace bibliotecha.Models
{
    public enum SortiranjePo
    {
        Ocjena,
        Naslov,
        NajnovijeIzdanje,
        NajstarijeIzdanje,
        Autor
    }

    public class TraziViewModel
    {
        public string? Upit { get; set; }
        public Zanr? Zanr { get; set; }
        public Jezik? Jezik { get; set; }
        public int? GodinaOd { get; set; }
        public int? GodinaDo { get; set; }
        public SortiranjePo Sortiranje { get; set; } = SortiranjePo.Ocjena;
        public List<Knjiga> Rezultati { get; set; } = new();
    }
}