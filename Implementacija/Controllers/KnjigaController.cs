using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using bibliotecha.Data;
using bibliotecha.Models;
using Microsoft.AspNetCore.Identity;

namespace bibliotecha.Controllers
{
    public class KnjigaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Korisnik> _userManager;

        public KnjigaController(ApplicationDbContext context, UserManager<Korisnik> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Knjiga
        [Authorize(Roles = "Bibliotekar,Administrator")]
        public async Task<IActionResult> Index(string? q)
        {
            var query = _context.Knjiga
                .Include(k => k.Autor)
                .Include(k => k.Primjerci)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();

                query = query.Where(k =>
                    k.Naslov.Contains(q) ||
                    k.ISBN.Contains(q) ||
                    k.Autor.Ime.Contains(q) ||
                    k.Autor.Prezime.Contains(q) ||
                    (k.Autor.Ime + " " + k.Autor.Prezime).Contains(q) ||
                    (k.Autor.Prezime + " " + k.Autor.Ime).Contains(q));
            }

            ViewData["Upit"] = q;

            return View(await query
                .OrderBy(k => k.Naslov)
                .ToListAsync());
        }

        // GET: Knjiga/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var knjiga = await _context.Knjiga
                .Include(k => k.Autor)
                .Include(k => k.Primjerci)
                .FirstOrDefaultAsync(m => m.IdKnjige == id);
            if (knjiga == null)
            {
                return NotFound();
            }
            bool imaAktivnuPosudbu = false;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var korisnik = await _userManager.GetUserAsync(User);

                if (korisnik != null)
                {
                    imaAktivnuPosudbu = await _context.Posudba
                        .Include(p => p.Primjerak)
                        .AnyAsync(p =>
                            p.KorisnikId == korisnik.Id &&
                            p.Primjerak.KnjigaId == knjiga.IdKnjige &&
                            (p.Status == StatusPosudbe.Online ||
                             p.Status == StatusPosudbe.Aktivna ||
                             p.Status == StatusPosudbe.Produzena));
                }
            }

            ViewData["ImaAktivnuPosudbu"] = imaAktivnuPosudbu;
            return View(knjiga);
        }

        // GET: Knjiga/Create
        [Authorize(Roles = "Bibliotekar,Administrator")]
        public IActionResult Create()
        {
            PopuniAutore();
            return View();
        }

        // POST: Knjiga/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Bibliotekar,Administrator")]
        public async Task<IActionResult> Create([Bind("IdKnjige,ISBN,Naslov,AutorId,Zanr,Opis,DatumIzdavanja,Izdavac,BrojStranica,Jezik,KoricaKnjige,ProsjecnaOcjena")] Knjiga knjiga)
        {
            ModelState.Remove(nameof(Knjiga.Autor));

            if (ModelState.IsValid)
            {
                _context.Add(knjiga);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopuniAutore(knjiga.AutorId);
            return View(knjiga);
        }

        // GET: Knjiga/Edit/5
        [Authorize(Roles = "Bibliotekar,Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var knjiga = await _context.Knjiga.FindAsync(id);
            if (knjiga == null)
            {
                return NotFound();
            }
            PopuniAutore(knjiga.AutorId);
            return View(knjiga);
        }

        // POST: Knjiga/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Bibliotekar,Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("IdKnjige,ISBN,Naslov,AutorId,Zanr,Opis,DatumIzdavanja,Izdavac,BrojStranica,Jezik,KoricaKnjige,ProsjecnaOcjena")] Knjiga knjiga)
        {
            if (id != knjiga.IdKnjige)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Knjiga.Autor));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(knjiga);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KnjigaExists(knjiga.IdKnjige))
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
            PopuniAutore(knjiga.AutorId);
            return View(knjiga);
        }

        // GET: Knjiga/Delete/5
        [Authorize(Roles = "Bibliotekar,Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var knjiga = await _context.Knjiga
                .Include(k => k.Autor)
                .FirstOrDefaultAsync(m => m.IdKnjige == id);
            if (knjiga == null)
            {
                return NotFound();
            }

            return View(knjiga);
        }

        // POST: Knjiga/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Bibliotekar,Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var knjiga = await _context.Knjiga.FindAsync(id);
            if (knjiga != null)
            {
                _context.Knjiga.Remove(knjiga);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KnjigaExists(int id)
        {
            return _context.Knjiga.Any(e => e.IdKnjige == id);
        }

        private void PopuniAutore(int? odabraniAutorId = null)
        {
            var autori = _context.Autor
                .OrderBy(a => a.Prezime)
                .ThenBy(a => a.Ime)
                .Select(a => new
                {
                    a.IdAutora,
                    PunoIme = a.Ime + " " + a.Prezime
                })
                .ToList();

            ViewData["AutorId"] = new SelectList(autori, "IdAutora", "PunoIme", odabraniAutorId);
        }
    }
}