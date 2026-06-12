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

namespace bibliotecha.Controllers
{
    public class RezervacijasController : Controller
    {
        private readonly ApplicationDbContext _context;
       private readonly UserManager<Korisnik> _userManager;

        public RezervacijasController(ApplicationDbContext context, UserManager<Korisnik> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Rezervacijas
        public async Task<IActionResult> Index(string? q)
        {
            var rezervacije = _context.Rezervacija
                .Include(r => r.Knjiga)
                    .ThenInclude(k => k.Autor)
                .Include(r => r.Korisnik)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.ToLower();

                rezervacije = rezervacije.Where(r =>
                    r.Knjiga.Naslov.ToLower().Contains(q) ||
                    r.Knjiga.ISBN.ToLower().Contains(q) ||
                    r.Knjiga.Autor.Ime.ToLower().Contains(q) ||
                    r.Knjiga.Autor.Prezime.ToLower().Contains(q) ||
                    r.Korisnik.Ime.ToLower().Contains(q) ||
                    r.Korisnik.Prezime.ToLower().Contains(q));
            }

            ViewData["Upit"] = q;

            return View(await rezervacije.ToListAsync());
        }

        // GET: Rezervacijas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezervacija = await _context.Rezervacija
                .Include(r => r.Knjiga)
                .Include(r => r.Korisnik)
                .FirstOrDefaultAsync(m => m.IdRezervacije == id);
            if (rezervacija == null)
            {
                return NotFound();
            }

            return View(rezervacija);
        }

        // GET: Rezervacijas/Create
        public IActionResult Create()
        {
            ViewData["KnjigaId"] = new SelectList(_context.Knjiga, "IdKnjige", "IdKnjige");
            ViewData["KorisnikId"] = new SelectList(_context.Korisnik, "IdKorisnika", "IdKorisnika");
            return View();
        }

        // POST: Rezervacijas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdRezervacije,KnjigaId,KorisnikId,DatumRezervacije,Status,PozicijaURedu")] Rezervacija rezervacija)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rezervacija);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["KnjigaId"] = new SelectList(_context.Knjiga, "IdKnjige", "IdKnjige", rezervacija.KnjigaId);
            ViewData["KorisnikId"] = new SelectList(_context.Korisnik, "IdKorisnika", "IdKorisnika", rezervacija.KorisnikId);
            return View(rezervacija);
        }

        // GET: Rezervacijas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezervacija = await _context.Rezervacija.FindAsync(id);
            if (rezervacija == null)
            {
                return NotFound();
            }
            ViewData["KnjigaId"] = new SelectList(_context.Knjiga, "IdKnjige", "IdKnjige", rezervacija.KnjigaId);
            ViewData["KorisnikId"] = new SelectList(_context.Korisnik, "IdKorisnika", "IdKorisnika", rezervacija.KorisnikId);
            return View(rezervacija);
        }

        // POST: Rezervacijas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdRezervacije,KnjigaId,KorisnikId,DatumRezervacije,Status,PozicijaURedu")] Rezervacija rezervacija)
        {
            if (id != rezervacija.IdRezervacije)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rezervacija);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RezervacijaExists(rezervacija.IdRezervacije))
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
            ViewData["KnjigaId"] = new SelectList(_context.Knjiga, "IdKnjige", "IdKnjige", rezervacija.KnjigaId);
            ViewData["KorisnikId"] = new SelectList(_context.Korisnik, "IdKorisnika", "IdKorisnika", rezervacija.KorisnikId);
            return View(rezervacija);
        }

        // GET: Rezervacijas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezervacija = await _context.Rezervacija
                .Include(r => r.Knjiga)
                .Include(r => r.Korisnik)
                .FirstOrDefaultAsync(m => m.IdRezervacije == id);
            if (rezervacija == null)
            {
                return NotFound();
            }

            return View(rezervacija);
        }

        // POST: Rezervacijas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rezervacija = await _context.Rezervacija.FindAsync(id);
            if (rezervacija != null)
            {
                _context.Rezervacija.Remove(rezervacija);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rezervisi(int knjigaId)
        {
            var korisnik = await _userManager.GetUserAsync(User);

            if (korisnik == null)
            {
                return Challenge();
            }

            var knjigaPostoji = await _context.Knjiga
                .AnyAsync(k => k.IdKnjige == knjigaId);

            if (!knjigaPostoji)
            {
                return NotFound();
            }

            var vecPostojiRezervacija = await _context.Rezervacija
                .AnyAsync(r =>
                    r.KnjigaId == knjigaId &&
                    r.KorisnikId == korisnik.Id &&
                    (r.Status == StatusRezervacije.Aktivna ||
                    r.Status == StatusRezervacije.spremnaZaPreuzimanje));

            if (vecPostojiRezervacija)
            {
                return RedirectToAction("Details", "Knjiga", new { id = knjigaId });
            }

            int pozicijaURedu = await _context.Rezervacija
                .CountAsync(r =>
                    r.KnjigaId == knjigaId &&
                    r.Status == StatusRezervacije.Aktivna) + 1;

            var rezervacija = new Rezervacija
            {
                KnjigaId = knjigaId,
                KorisnikId = korisnik.Id,
                DatumRezervacije = DateOnly.FromDateTime(DateTime.Now),
                Status = StatusRezervacije.Aktivna,
                PozicijaURedu = pozicijaURedu
            };

            _context.Rezervacija.Add(rezervacija);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Knjiga", new { id = knjigaId });
        }

        private bool RezervacijaExists(int id)
        {
            return _context.Rezervacija.Any(e => e.IdRezervacije == id);
        }
    }
}