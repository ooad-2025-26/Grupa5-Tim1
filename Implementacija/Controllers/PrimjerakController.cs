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
    public class PrimjerakController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrimjerakController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Primjerak
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Primjerak.Include(p => p.Knjiga);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Primjerak/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var primjerak = await _context.Primjerak
                .Include(p => p.Knjiga)
                .FirstOrDefaultAsync(m => m.IdPrimjerka == id);
            if (primjerak == null)
            {
                return NotFound();
            }

            return View(primjerak);
        }

        // GET: Primjerak/Create
        public IActionResult Create()
        {
            ViewData["KnjigaId"] = new SelectList(_context.Knjiga, "IdKnjige", "IdKnjige");
            return View();
        }

        // POST: Primjerak/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPrimjerka,Status,KnjigaId")] Primjerak primjerak)
        {
            if (ModelState.IsValid)
            {
                _context.Add(primjerak);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["KnjigaId"] = new SelectList(_context.Knjiga, "IdKnjige", "IdKnjige", primjerak.KnjigaId);
            return View(primjerak);
        }

        // GET: Primjerak/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var primjerak = await _context.Primjerak.FindAsync(id);
            if (primjerak == null)
            {
                return NotFound();
            }
            ViewData["KnjigaId"] = new SelectList(_context.Knjiga, "IdKnjige", "IdKnjige", primjerak.KnjigaId);
            return View(primjerak);
        }

        // POST: Primjerak/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPrimjerka,Status,KnjigaId")] Primjerak primjerak)
        {
            if (id != primjerak.IdPrimjerka)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(primjerak);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrimjerakExists(primjerak.IdPrimjerka))
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
            ViewData["KnjigaId"] = new SelectList(_context.Knjiga, "IdKnjige", "IdKnjige", primjerak.KnjigaId);
            return View(primjerak);
        }

        // GET: Primjerak/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var primjerak = await _context.Primjerak
                .Include(p => p.Knjiga)
                .FirstOrDefaultAsync(m => m.IdPrimjerka == id);
            if (primjerak == null)
            {
                return NotFound();
            }

            return View(primjerak);
        }

        // POST: Primjerak/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var primjerak = await _context.Primjerak.FindAsync(id);
            if (primjerak != null)
            {
                _context.Primjerak.Remove(primjerak);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrimjerakExists(int id)
        {
            return _context.Primjerak.Any(e => e.IdPrimjerka == id);
        }
    }
}
