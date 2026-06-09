using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using bibliotecha.Data;
using bibliotecha.Models;

namespace bibliotecha.Controllers
{
    public class PosudbaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PosudbaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Posudba
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Posudba.Include(p => p.Korisnik).Include(p => p.Primjerak);
            return View(await applicationDbContext.ToListAsync());
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
            ViewData["KorisnikId"] = new SelectList(_context.Korisnik, "IdKorisnika", "IdKorisnika");
            ViewData["PrimjerakId"] = new SelectList(_context.Primjerak, "IdPrimjerka", "IdPrimjerka");
            return View();
        }

        // POST: Posudba/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPosudbe,PrimjerakId,KorisnikId,DatumOnlinePosudbe,DatumPreuzimanja,RokVracanja,Status,BrojProduzenja")] Posudba posudba)
        {
            if (ModelState.IsValid)
            {
                _context.Add(posudba);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["KorisnikId"] = new SelectList(_context.Korisnik, "IdKorisnika", "IdKorisnika", posudba.KorisnikId);
            ViewData["PrimjerakId"] = new SelectList(_context.Primjerak, "IdPrimjerka", "IdPrimjerka", posudba.PrimjerakId);
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

        private bool PosudbaExists(int id)
        {
            return _context.Posudba.Any(e => e.IdPosudbe == id);
        }
    }
}
