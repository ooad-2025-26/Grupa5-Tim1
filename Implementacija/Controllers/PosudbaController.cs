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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Bibliotekar,Administrator")]
        public async Task<IActionResult> Create(
    [Bind("PrimjerakId,KorisnikId,RokVracanja")]
    Posudba posudba)
        {
            var primjerak = await _context.Primjerak
                .FirstOrDefaultAsync(p => p.IdPrimjerka == posudba.PrimjerakId);

            if (primjerak == null)
            {
                ModelState.AddModelError("", "Odabrani primjerak ne postoji.");
            }

            Rezervacija? spremnaRezervacija = null;

            if (primjerak?.Status == StatusPrimjerka.Rezervisan)
            {
                spremnaRezervacija = await _context.Rezervacija
                    .FirstOrDefaultAsync(r =>
                        r.KnjigaId == primjerak.KnjigaId &&
                        r.KorisnikId == posudba.KorisnikId &&
                        r.Status == StatusRezervacije.spremnaZaPreuzimanje);

                if (spremnaRezervacija == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Rezervisani primjerak može preuzeti samo korisnik kojem je dodijeljen.");
                }
            }
            else if (primjerak?.Status != StatusPrimjerka.Dostupan)
            {
                ModelState.AddModelError("", "Primjerak nije dostupan za posudbu.");
            }

            if (ModelState.IsValid)
            {
                var danas = DateOnly.FromDateTime(DateTime.Today);

                posudba.DatumOnlinePosudbe = danas;
                posudba.DatumPreuzimanja = danas;
                posudba.RokVracanja =
                    danas.AddDays(_notificationSettings.LoanDurationDays);

                posudba.Status = StatusPosudbe.Aktivna;
                posudba.BrojProduzenja = 0;

                primjerak!.Status = StatusPrimjerka.Posudjen;
                if (spremnaRezervacija != null)
                {
                    spremnaRezervacija.Status = StatusRezervacije.Ispunjena;
                }

                _context.Posudba.Add(posudba);

                await _context.SaveChangesAsync();

                TempData["Poruka"] = "Nova posudba je uspješno evidentirana.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.PrimjerakId = new SelectList(
    _context.Primjerak
        .Where(p =>
            p.Status == StatusPrimjerka.Dostupan ||
            p.Status == StatusPrimjerka.Rezervisan),
                "IdPrimjerka",
                "IdPrimjerka",
                posudba.PrimjerakId);

            ViewBag.KorisnikId = new SelectList(
                _context.Korisnik
                    .OrderBy(k => k.Prezime),
                "Id",
                "Email",
                posudba.KorisnikId);

            return View(posudba);
        }

        // GET: Posudba/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var posudba = await _context.Posudba.FindAsync(id);
            if (posudba == null)
            {
                return NotFound();
            }
            ViewData["KorisnikId"] = new SelectList(_context.Korisnik, "IdKorisnika", "IdKorisnika", posudba.KorisnikId);
            ViewData["PrimjerakId"] = new SelectList(_context.Primjerak, "IdPrimjerka", "IdPrimjerka", posudba.PrimjerakId);
            return View(posudba);
        }

        // POST: Posudba/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPosudbe,PrimjerakId,KorisnikId,DatumOnlinePosudbe,DatumPreuzimanja,RokVracanja,Status,BrojProduzenja")] Posudba posudba)
        {
            if (id != posudba.IdPosudbe)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(posudba);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PosudbaExists(posudba.IdPosudbe))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["KorisnikId"] = new SelectList(_context.Korisnik, "IdKorisnika", "IdKorisnika", posudba.KorisnikId);
            ViewData["PrimjerakId"] = new SelectList(_context.Primjerak, "IdPrimjerka", "IdPrimjerka", posudba.PrimjerakId);
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

        private bool PosudbaExists(int id)
        {
            return _context.Posudba.Any(e => e.IdPosudbe == id);
        }
    }
}