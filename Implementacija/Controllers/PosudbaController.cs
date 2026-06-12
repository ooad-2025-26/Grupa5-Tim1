using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using bibliotecha.Data;
using bibliotecha.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using bibliotecha.Services;
using Microsoft.Extensions.Options;

namespace bibliotecha.Controllers
{
    public class PosudbaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;
        private readonly NotificationSettings _notificationSettings;

        public PosudbaController(
            ApplicationDbContext context,
            UserManager<Korisnik> userManager,
            IOptions<NotificationSettings> notificationSettings)
        {
            _context = context;
            _userManager = userManager;
            _notificationSettings = notificationSettings.Value;
        }

        // GET: Posudba
        public async Task<IActionResult> Index(string? q)
        {
            var posudbe = _context.Posudba
                .Include(p => p.Korisnik)
                .Include(p => p.Primjerak)
                    .ThenInclude(pr => pr.Knjiga)
                        .ThenInclude(k => k.Autor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.ToLower();

                posudbe = posudbe.Where(p =>
                    p.Primjerak.Knjiga.Naslov.ToLower().Contains(q) ||
                    p.Primjerak.Knjiga.ISBN.ToLower().Contains(q) ||
                    p.Primjerak.Knjiga.Autor.Ime.ToLower().Contains(q) ||
                    p.Primjerak.Knjiga.Autor.Prezime.ToLower().Contains(q) ||
                    p.Korisnik.Ime.ToLower().Contains(q) ||
                    p.Korisnik.Prezime.ToLower().Contains(q));
            }

            ViewData["Upit"] = q;

            return View(await posudbe.ToListAsync());
        }

        // GET: Posudba/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posudba = await _context.Posudba
                .Include(p => p.Korisnik)
                .Include(p => p.Primjerak)
                      .ThenInclude(pr => pr.Knjiga)
                .FirstOrDefaultAsync(m => m.IdPosudbe == id);
            if (posudba == null)
            {
                return NotFound();
            }

            return View(posudba);
        }

        // GET: Posudba/Create
        public IActionResult Create()
        {
            ViewData["KorisnikId"] = new SelectList(
                _context.Korisnik
                    .OrderBy(k => k.Prezime)
                    .Select(k => new
                    {
                        k.Id,
                        Naziv = k.Ime + " " + k.Prezime +
                                 " (Kartica: " + k.BrojClanskeKartice + ")"
                    }),
                "Id",
                "Naziv");

            ViewData["PrimjerakId"] = new SelectList(
    _context.Primjerak
        .Include(p => p.Knjiga)
        .Where(p =>
            p.Status == StatusPrimjerka.Dostupan ||
            p.Status == StatusPrimjerka.Rezervisan)
        .Select(p => new
        {
            p.IdPrimjerka,
            Naziv = "#" + p.IdPrimjerka +
                    " - " +
                    p.Knjiga.Naslov +
                    (p.Status == StatusPrimjerka.Rezervisan
                        ? " (rezervisan)"
                        : "")
        }),
    "IdPrimjerka",
    "Naziv");

            return View();
        }

        // POST: Posudba/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Bibliotekar,Administrator")]
        public async Task<IActionResult> Create(
            [Bind("PrimjerakId,KorisnikId,DatumPreuzimanja")]
    Posudba posudba)
        {
            var primjerak = await _context.Primjerak
                .Include(p => p.Knjiga)
                .FirstOrDefaultAsync(p => p.IdPrimjerka == posudba.PrimjerakId);


            if (primjerak == null)
            {
                ModelState.AddModelError("", "Odabrani primjerak ne postoji.");
            }
            else
            {
                // provjera statusa primjerka
                if (primjerak.Status == StatusPrimjerka.Posudjen)
                {
                    ModelState.AddModelError("",
                        "Ovaj primjerak je već posuđen. Nije moguće napraviti novu posudbu.");
                }


                if (primjerak.Status == StatusPrimjerka.Rezervisan)
                {
                    var rezervacija = await _context.Rezervacija
                        .FirstOrDefaultAsync(r =>
                            r.KnjigaId == primjerak.KnjigaId &&
                            r.Status == StatusRezervacije.spremnaZaPreuzimanje);


                    if (rezervacija != null &&
                        rezervacija.KorisnikId != posudba.KorisnikId)
                    {
                        ModelState.AddModelError("",
                            "Ovaj primjerak je rezervisan za drugog korisnika.");
                    }
                }
            }


            if (ModelState.IsValid)
            {
                var danas = posudba.DatumPreuzimanja
                    ?? DateOnly.FromDateTime(DateTime.Today);


                posudba.DatumPreuzimanja = danas;
                posudba.DatumOnlinePosudbe = null;

                posudba.RokVracanja =
                    danas.AddDays(_notificationSettings.LoanDurationDays);

                posudba.Status = StatusPosudbe.Aktivna;

                posudba.BrojProduzenja = 0;


                primjerak!.Status = StatusPrimjerka.Posudjen;


                // ako je bio rezervisan, označi rezervaciju kao završenu
                var spremnaRezervacija = await _context.Rezervacija
                    .FirstOrDefaultAsync(r =>
                        r.KnjigaId == primjerak.KnjigaId &&
                        r.KorisnikId == posudba.KorisnikId &&
                        r.Status == StatusRezervacije.spremnaZaPreuzimanje);


                if (spremnaRezervacija != null)
                {
                    spremnaRezervacija.Status = StatusRezervacije.Ispunjena;
                }


                _context.Posudba.Add(posudba);

                await _context.SaveChangesAsync();


                TempData["Poruka"] =
                    "Nova posudba je uspješno evidentirana.";

                return RedirectToAction(nameof(Index));
            }



            // ako validacija padne
            ViewData["KorisnikId"] = new SelectList(
                _context.Korisnik
                    .OrderBy(k => k.Prezime)
                    .Select(k => new
                    {
                        k.Id,
                        Naziv = k.Ime + " " + k.Prezime +
                                " (Kartica: " + k.BrojClanskeKartice + ")"
                    }),
                "Id",
                "Naziv",
                posudba.KorisnikId
            );


            ViewData["PrimjerakId"] = new SelectList(
                _context.Primjerak
                    .Include(p => p.Knjiga)
                    .Where(p =>
                        p.Status == StatusPrimjerka.Dostupan ||
                        p.IdPrimjerka == posudba.PrimjerakId)
                    .Select(p => new
                    {
                        p.IdPrimjerka,
                        Naziv = "#" + p.IdPrimjerka +
                                " - " + p.Knjiga.Naslov
                    }),
                "IdPrimjerka",
                "Naziv",
                posudba.PrimjerakId
            );


            return View(posudba);
        }

        // GET: Posudba/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posudba = await _context.Posudba
                .Include(p => p.Primjerak)
                    .ThenInclude(pr => pr.Knjiga)
                .Include(p => p.Korisnik)
                .FirstOrDefaultAsync(p => p.IdPosudbe == id);

            if (posudba == null)
            {
                return NotFound();
            }


            ViewData["KorisnikId"] = new SelectList(
                _context.Korisnik
                    .OrderBy(k => k.Prezime)
                    .Select(k => new
                    {
                        k.Id,
                        Naziv = k.Ime + " " + k.Prezime +
                                " (Kartica: " + k.BrojClanskeKartice + ")"
                    }),
                "Id",
                "Naziv",
                posudba.KorisnikId
            );


            ViewData["PrimjerakId"] = new SelectList(
                _context.Primjerak
                    .Include(p => p.Knjiga)
                    .Select(p => new
                    {
                        p.IdPrimjerka,
                        Naziv = "#" + p.IdPrimjerka +
                                " - " +
                                p.Knjiga.Naslov
                    }),
                "IdPrimjerka",
                "Naziv",
                posudba.PrimjerakId
            );


            return View(posudba);
        }

        // POST: Posudba/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("IdPosudbe,PrimjerakId,KorisnikId,DatumOnlinePosudbe,DatumPreuzimanja,RokVracanja,Status,BrojProduzenja")] Posudba posudba)
        {
            if (id != posudba.IdPosudbe)
            {
                return NotFound();
            }


            // ignoriši navigacione validacije
            ModelState.Remove(nameof(Posudba.Korisnik));
            ModelState.Remove(nameof(Posudba.Primjerak));


            if (ModelState.IsValid)
            {
                try
                {
                    var postojecaPosudba = await _context.Posudba
                        .FirstOrDefaultAsync(p => p.IdPosudbe == id);


                    if (postojecaPosudba == null)
                    {
                        return NotFound();
                    }


                    postojecaPosudba.PrimjerakId = posudba.PrimjerakId;
                    postojecaPosudba.KorisnikId = posudba.KorisnikId;

                    postojecaPosudba.DatumOnlinePosudbe =
                        posudba.DatumOnlinePosudbe;

                    postojecaPosudba.DatumPreuzimanja =
                        posudba.DatumPreuzimanja;

                    postojecaPosudba.RokVracanja =
                        posudba.RokVracanja;

                    postojecaPosudba.Status =
                        posudba.Status;

                    postojecaPosudba.BrojProduzenja =
                        posudba.BrojProduzenja;


                    await _context.SaveChangesAsync();


                    TempData["Poruka"] =
                        "Posudba je uspješno izmijenjena.";


                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PosudbaExists(posudba.IdPosudbe))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }


            // ako validacija padne, ponovo puni selecte

            ViewData["KorisnikId"] = new SelectList(
                _context.Korisnik
                    .OrderBy(k => k.Prezime)
                    .Select(k => new
                    {
                        k.Id,
                        Naziv = k.Ime + " " + k.Prezime +
                                " (Kartica: " + k.BrojClanskeKartice + ")"
                    }),
                "Id",
                "Naziv",
                posudba.KorisnikId
            );


            ViewData["PrimjerakId"] = new SelectList(
                _context.Primjerak
                    .Include(p => p.Knjiga)
                    .Select(p => new
                    {
                        p.IdPrimjerka,
                        Naziv = "#" + p.IdPrimjerka +
                                " - " +
                                p.Knjiga.Naslov
                    }),
                "IdPrimjerka",
                "Naziv",
                posudba.PrimjerakId
            );


            return View(posudba);
        }

        // GET: Posudba/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posudba = await _context.Posudba
                .Include(p => p.Korisnik)
                .Include(p => p.Primjerak)
                .FirstOrDefaultAsync(m => m.IdPosudbe == id);
            if (posudba == null)
            {
                return NotFound();
            }

            return View(posudba);
        }

        // POST: Posudba/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var posudba = await _context.Posudba.FindAsync(id);
            if (posudba != null)
            {
                _context.Posudba.Remove(posudba);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Posudi(int knjigaId)
        {
            var korisnik = await _userManager.GetUserAsync(User);

            if (korisnik == null)
            {
                return Challenge();
            }

            var dostupanPrimjerak = await _context.Primjerak
                .FirstOrDefaultAsync(p =>
                    p.KnjigaId == knjigaId &&
                    p.Status == StatusPrimjerka.Dostupan);

            if (dostupanPrimjerak == null)
            {
                TempData["NemaDostupnihPrimjeraka"] = true;
                return RedirectToAction("Details", "Knjiga", new { id = knjigaId });
            }

            var danas = DateOnly.FromDateTime(DateTime.Now);

            var posudba = new Posudba
            {
                PrimjerakId = dostupanPrimjerak.IdPrimjerka,
                KorisnikId = korisnik.Id,
                DatumOnlinePosudbe = danas,
                DatumPreuzimanja = null,
                RokVracanja = danas.AddDays(_notificationSettings.LoanDurationDays),
                Status = StatusPosudbe.Online,
                BrojProduzenja = 0
            };

            dostupanPrimjerak.Status = StatusPrimjerka.Posudjen;

            _context.Posudba.Add(posudba);
            await _context.SaveChangesAsync();

            TempData["Poruka"] = "Posudba je evidentirana.";

            return RedirectToAction("Details", "Knjiga", new { id = knjigaId });
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Produzi(int knjigaId)
        {
            var korisnik = await _userManager.GetUserAsync(User);

            if (korisnik == null)
            {
                return Challenge();
            }

            var posudba = await _context.Posudba
                .Include(p => p.Primjerak)
                .FirstOrDefaultAsync(p =>
                    p.KorisnikId == korisnik.Id &&
                    p.Primjerak.KnjigaId == knjigaId &&
                    (p.Status == StatusPosudbe.Online ||
                     p.Status == StatusPosudbe.Aktivna ||
                     p.Status == StatusPosudbe.Produzena));

            if (posudba == null)
            {
                TempData["Poruka"] = "Nemate aktivnu posudbu za ovu knjigu.";
                return RedirectToAction("Details", "Knjiga", new { id = knjigaId });
            }

            if (posudba.BrojProduzenja >= 1)
            {
                TempData["Poruka"] = "Posudbu nije moguće produžiti više od jedanput.";
                return RedirectToAction("Details", "Knjiga", new { id = knjigaId });
            }

            bool postojiAktivnaRezervacija = await _context.Rezervacija
                .AnyAsync(r =>
                    r.KnjigaId == knjigaId &&
                    (r.Status == StatusRezervacije.Aktivna ||
                    r.Status == StatusRezervacije.spremnaZaPreuzimanje));

            if (postojiAktivnaRezervacija)
            {
                TempData["Poruka"] = "Posudbu nije moguće produžiti jer postoji aktivna rezervacija za ovu knjigu.";
                return RedirectToAction("Details", "Knjiga", new { id = knjigaId });
            }

            posudba.RokVracanja = posudba.RokVracanja.AddDays(7);
            posudba.BrojProduzenja = (posudba.BrojProduzenja ?? 0) + 1;
            posudba.Status = StatusPosudbe.Produzena;

            await _context.SaveChangesAsync();

            TempData["Poruka"] = "Posudba je uspješno produžena.";

            return RedirectToAction("Details", "Knjiga", new { id = knjigaId });
        }

        [Authorize(Roles = "Bibliotekar,Administrator")]
        public async Task<IActionResult> Preuzmi(int id)
        {
            var posudba = await _context.Posudba
                .Include(p => p.Primjerak)
                .FirstOrDefaultAsync(p => p.IdPosudbe == id);

            if (posudba == null)
                return NotFound();

            if (posudba.Status != StatusPosudbe.Online)
            {
                TempData["Poruka"] = "Posudba nije u statusu Online.";
                return RedirectToAction(nameof(Index));
            }

            posudba.DatumPreuzimanja = DateOnly.FromDateTime(DateTime.Today);

            posudba.RokVracanja = posudba.DatumPreuzimanja.Value
                .AddDays(_notificationSettings.LoanDurationDays);

            posudba.Status = StatusPosudbe.Aktivna;

            await _context.SaveChangesAsync();

            TempData["Poruka"] = "Knjiga je preuzeta.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Bibliotekar,Administrator")]
        public async Task<IActionResult> Vrati(int id)
        {
            var posudba = await _context.Posudba
                .Include(p => p.Primjerak)
                .FirstOrDefaultAsync(p => p.IdPosudbe == id);

            if (posudba == null)
                return NotFound();

            if (posudba.Status == StatusPosudbe.Zavrsena)
            {
                TempData["Poruka"] = "Posudba je već završena.";
                return RedirectToAction(nameof(Index));
            }

            posudba.Status = StatusPosudbe.Zavrsena;

            posudba.Primjerak.Status = StatusPrimjerka.Dostupan;

            await _context.SaveChangesAsync();

            TempData["Poruka"] = "Knjiga je vraćena.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> PrimjerciZaKnjigu(int knjigaId)
{
    var primjerci = await _context.Primjerak
        .Where(p => p.KnjigaId == knjigaId)
        .Select(p => new
        {
            id = p.IdPrimjerka,
            naziv = "#" + p.IdPrimjerka
        })
        .ToListAsync();

    return Json(primjerci);
}
        private bool PosudbaExists(int id)
        {
            return _context.Posudba.Any(e => e.IdPosudbe == id);
        }
    }
}