using bibliotecha.Models;
using bibliotecha.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace bibliotecha.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var knjige = await _context.Knjiga
                .Include(knjiga => knjiga.Autor)
                .OrderByDescending(knjiga => knjiga.ProsjecnaOcjena)
                .ThenBy(knjiga => knjiga.Naslov)
                .ToListAsync();

            var sekcije = knjige
                .GroupBy(knjiga => knjiga.Zanr)
                .OrderBy(grupa => grupa.Key)
                .Select(grupa => new KnjigaBlokViewModel
                {
                    Zanr = grupa.Key,
                    NazivZanra = FormatirajNazivZanra(grupa.Key),
                    Knjige = grupa.ToList()
                })
                .ToList();

            return View(sekcije);
        }

        public async Task<IActionResult> Trazi(string? q, Zanr? zanr, Jezik? jezik, int? godinaOd, int? godinaDo, SortiranjePo sortiranje = SortiranjePo.Naslov)
        {
            var sveKnjige = await _context.Knjiga
                .Include(k => k.Autor)
                .ToListAsync();

            var rezultati = new List<Knjiga>();

            foreach (var knjiga in sveKnjige)
            {
                bool odgovara = true;

                if (!string.IsNullOrWhiteSpace(q))
                {
                    string upit = q.ToLower();

                    bool naslovOdgovara = knjiga.Naslov.ToLower().Contains(upit);
                    bool autorOdgovara = (knjiga.Autor.Ime + " " + knjiga.Autor.Prezime)
                        .ToLower()
                        .Contains(upit);

                    odgovara = naslovOdgovara || autorOdgovara;
                }

                if (odgovara && zanr.HasValue && knjiga.Zanr != zanr.Value)
                    odgovara = false;

                if (odgovara && jezik.HasValue && knjiga.Jezik != jezik.Value)
                    odgovara = false;

                if (odgovara && godinaOd.HasValue && knjiga.DatumIzdavanja.Year < godinaOd.Value)
                    odgovara = false;

                if (odgovara && godinaDo.HasValue && knjiga.DatumIzdavanja.Year > godinaDo.Value)
                    odgovara = false;

                if (odgovara)
                    rezultati.Add(knjiga);
            }

            rezultati = SortirajKnjige(rezultati, sortiranje);

            var vm = new TraziViewModel
            {
                Upit = q,
                Zanr = zanr,
                Jezik = jezik,
                GodinaOd = godinaOd,
                GodinaDo = godinaDo,
                Sortiranje = sortiranje,
                Rezultati = rezultati
            };

            return View(vm);
        }

        private static List<Knjiga> SortirajKnjige(List<Knjiga> knjige, SortiranjePo sortiranje)
        {
            for (int i = 0; i < knjige.Count - 1; i++)
            {
                for (int j = i + 1; j < knjige.Count; j++)
                {
                    if (TrebaZamijeniti(knjige[i], knjige[j], sortiranje))
                    {
                        var temp = knjige[i];
                        knjige[i] = knjige[j];
                        knjige[j] = temp;
                    }
                }
            }

            return knjige;
        }

        private static bool TrebaZamijeniti(Knjiga prva, Knjiga druga, SortiranjePo sortiranje)
        {
            return sortiranje switch
            {
                SortiranjePo.Naslov =>
                    string.Compare(prva.Naslov, druga.Naslov, StringComparison.OrdinalIgnoreCase) > 0,

                SortiranjePo.Ocjena =>
                    prva.ProsjecnaOcjena < druga.ProsjecnaOcjena,

                SortiranjePo.NajnovijeIzdanje =>
                    prva.DatumIzdavanja < druga.DatumIzdavanja,

                SortiranjePo.NajstarijeIzdanje =>
                    prva.DatumIzdavanja > druga.DatumIzdavanja,

                SortiranjePo.Autor =>
                    string.Compare(
                        prva.Autor.Prezime + prva.Autor.Ime,
                        druga.Autor.Prezime + druga.Autor.Ime,
                        StringComparison.OrdinalIgnoreCase) > 0,

                _ => false
            };
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static string FormatirajNazivZanra(Zanr zanr)
        {
            return zanr switch
            {
                Zanr.Klasik => "Klasici",
                Zanr.Naucne => "Naučne knjige",
                Zanr.Djecije => "Dječije knjige",
                _ => zanr.ToString()
            };
        }
    }
}