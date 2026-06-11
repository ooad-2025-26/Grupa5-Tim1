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
            var query = _context.Knjiga
                .Include(k => k.Autor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(k =>
                    k.Naslov.Contains(q) ||
                    k.Autor.Ime.Contains(q) ||
                    k.Autor.Prezime.Contains(q));

            if (zanr.HasValue)
                query = query.Where(k => k.Zanr == zanr.Value);

            if (jezik.HasValue)
                query = query.Where(k => k.Jezik == jezik.Value);

            if (godinaOd.HasValue)
                query = query.Where(k => k.DatumIzdavanja.Year >= godinaOd.Value);

            if (godinaDo.HasValue)
                query = query.Where(k => k.DatumIzdavanja.Year <= godinaDo.Value);

            query = sortiranje switch
            {
                SortiranjePo.Naslov => query.OrderBy(k => k.Naslov),
                SortiranjePo.NajnovijeIzdanje => query.OrderByDescending(k => k.DatumIzdavanja),
                SortiranjePo.NajstarijeIzdanje => query.OrderBy(k => k.DatumIzdavanja),
                SortiranjePo.Autor => query.OrderBy(k => k.Autor.Prezime).ThenBy(k => k.Autor.Ime),
                SortiranjePo.Ocjena => query.OrderByDescending(k => k.ProsjecnaOcjena).ThenBy(k => k.Naslov),
                _ => query.OrderBy(k => k.Naslov)
            };

            var rezultati = await query.ToListAsync();

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
                Zanr.Naucne => "Naucne knjige",
                Zanr.Djecije => "Djecije knjige",
                _ => zanr.ToString()
            };
        }
    }
}