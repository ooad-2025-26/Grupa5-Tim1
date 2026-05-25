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
