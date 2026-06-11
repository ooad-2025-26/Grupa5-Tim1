using bibliotecha.Models;
using bibliotecha.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bibliotecha.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class KorisnikController : Controller
    {
        private readonly UserManager<Korisnik> _userManager;

        public KorisnikController(UserManager<Korisnik> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? q, Uloga? uloga, bool? aktivan)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(k =>
                    k.Ime.Contains(q) ||
                    k.Prezime.Contains(q) ||
                    (k.Email != null && k.Email.Contains(q)));
            }

            if (uloga.HasValue)
            {
                query = query.Where(k => k.Uloga == uloga.Value);
            }

            var sada = DateTimeOffset.UtcNow;
            if (aktivan == true)
            {
                query = query.Where(k => !k.LockoutEnd.HasValue || k.LockoutEnd <= sada);
            }
            else if (aktivan == false)
            {
                query = query.Where(k => k.LockoutEnd.HasValue && k.LockoutEnd > sada);
            }

            var model = new KorisnikListaViewModel
            {
                Korisnici = await query
                    .OrderBy(k => k.Prezime)
                    .ThenBy(k => k.Ime)
                    .ToListAsync(),
                Upit = q,
                Uloga = uloga,
                Aktivan = aktivan
            };

            return View(model);
        }

        public IActionResult Create()
        {
            return View(new KreirajKorisnikaViewModel
            {
                Uloga = Uloga.Bibliotekar
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KreirajKorisnikaViewModel model)
        {
            if (model.Uloga == Uloga.User)
            {
                ModelState.AddModelError(
                    nameof(model.Uloga),
                    "Administratorska forma kreira samo bibliotekarske i administratorske naloge.");
            }
            var postojeciKorisnik = await _userManager.FindByEmailAsync(model.Email);

            if (string.IsNullOrWhiteSpace(model.Lozinka))
            {
                ModelState.AddModelError(
                    nameof(model.Lozinka),
                    "Lozinka je obavezna.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (postojeciKorisnik != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Korisnički nalog s ovom email adresom već postoji.");

                return View(model);
            }

            var korisnik = new Korisnik
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                Ime = model.Ime.Trim(),
                Prezime = model.Prezime.Trim(),
                BrojClanskeKartice = null,
                DatumRegistracije = DateOnly.FromDateTime(DateTime.Today),
                DatumZaposlenja = DateOnly.FromDateTime(DateTime.Today),
                Uloga = model.Uloga,
                LockoutEnabled = true
            };

            var rezultat = await _userManager.CreateAsync(korisnik, model.Lozinka!);
            if (!rezultat.Succeeded)
            {
                DodajGreske(rezultat);
                return View(model);
            }

            var rezultatUlogeNovog = await AzurirajUlogu(korisnik, model.Uloga);
            if (!rezultatUlogeNovog.Succeeded)
            {
                DodajGreske(rezultatUlogeNovog);
                return View(model);
            }

            TempData["Poruka"] = "Novi korisnički nalog je uspješno kreiran.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var korisnik = await _userManager.FindByIdAsync(id);
            if (korisnik == null)
            {
                return NotFound();
            }

            return View(new UrediKorisnikaViewModel
            {
                Id = korisnik.Id,
                Ime = korisnik.Ime,
                Prezime = korisnik.Prezime,
                Email = korisnik.Email ?? string.Empty,
                Uloga = korisnik.Uloga,
                BrojClanskeKartice = korisnik.BrojClanskeKartice,
                DatumZaposlenja = korisnik.DatumZaposlenja
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UrediKorisnikaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var korisnik = await _userManager.FindByIdAsync(model.Id);
            if (korisnik == null)
            {
                return NotFound();
            }

            var korisnikSaEmailom = await _userManager.FindByEmailAsync(model.Email);
            if (korisnikSaEmailom != null && korisnikSaEmailom.Id != korisnik.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "Drugi nalog već koristi ovu email adresu.");
                return View(model);
            }

            korisnik.Ime = model.Ime.Trim();
            korisnik.Prezime = model.Prezime.Trim();
            korisnik.BrojClanskeKartice = model.BrojClanskeKartice;
            korisnik.Uloga = model.Uloga;
            korisnik.DatumZaposlenja = model.Uloga is Uloga.Bibliotekar or Uloga.Administrator
                ? model.DatumZaposlenja ?? DateOnly.FromDateTime(DateTime.Today)
                : null;

            if (!string.Equals(korisnik.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                await _userManager.SetEmailAsync(korisnik, model.Email.Trim());
                await _userManager.SetUserNameAsync(korisnik, model.Email.Trim());
            }

            var rezultat = await _userManager.UpdateAsync(korisnik);
            if (!rezultat.Succeeded)
            {
                DodajGreske(rezultat);
                return View(model);
            }

            var rezultatUloge = await AzurirajUlogu(korisnik, model.Uloga);
            if (!rezultatUloge.Succeeded)
            {
                DodajGreske(rezultatUloge);
                return View(model);
            }

            TempData["Poruka"] = "Podaci korisničkog naloga su uspješno izmijenjeni.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromijeniStatus(string id)
        {
            var korisnik = await _userManager.FindByIdAsync(id);
            if (korisnik == null)
            {
                return NotFound();
            }

            var deaktiviran = korisnik.LockoutEnd.HasValue &&
                korisnik.LockoutEnd > DateTimeOffset.UtcNow;

            korisnik.LockoutEnabled = true;
            korisnik.LockoutEnd = deaktiviran ? null : DateTimeOffset.MaxValue;

            var rezultat = await _userManager.UpdateAsync(korisnik);
            if (!rezultat.Succeeded)
            {
                TempData["Greska"] = "Status korisničkog naloga nije moguće promijeniti.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Poruka"] = deaktiviran
                ? "Korisnički nalog je ponovo aktiviran."
                : "Korisnički nalog je deaktiviran.";

            return RedirectToAction(nameof(Index));
        }

        private void DodajGreske(IdentityResult rezultat)
        {
            foreach (var greska in rezultat.Errors)
            {
                ModelState.AddModelError(string.Empty, greska.Description);
            }
        }

        private async Task<IdentityResult> AzurirajUlogu(Korisnik korisnik, Uloga uloga)
        {
            var postojeceUloge = await _userManager.GetRolesAsync(korisnik);

            if (postojeceUloge.Count > 0)
            {
                var rezultatUklanjanja =
                    await _userManager.RemoveFromRolesAsync(korisnik, postojeceUloge);

                if (!rezultatUklanjanja.Succeeded)
                {
                    return rezultatUklanjanja;
                }
            }

            return await _userManager.AddToRoleAsync(korisnik, uloga.ToString());
        }
    }
}
