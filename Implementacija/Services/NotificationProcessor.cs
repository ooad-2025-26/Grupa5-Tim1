using bibliotecha.Data;
using bibliotecha.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace bibliotecha.Services
{
    public class NotificationProcessor
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly NotificationSettings _settings;
        private readonly ILogger<NotificationProcessor> _logger;

        public NotificationProcessor(
            ApplicationDbContext context,
            IEmailService emailService,
            IOptions<NotificationSettings> settings,
            ILogger<NotificationProcessor> logger)
        {
            _context = context;
            _emailService = emailService;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task ProcessAsync(CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            await ProcessOverdueLoansAsync(today, cancellationToken);
            await ExpireUnclaimedReservationsAsync(today, cancellationToken);
            await NotifyReservationQueueAsync(today, cancellationToken);
            await ReorderActiveReservationsAsync(cancellationToken);
        }

        private async Task ProcessOverdueLoansAsync(
            DateOnly today,
            CancellationToken cancellationToken)
        {
            var overdueLoans = await _context.Posudba
                .Include(p => p.Korisnik)
                .Include(p => p.Primjerak)
                    .ThenInclude(p => p.Knjiga)
                .Where(p =>
                    p.RokVracanja < today &&
                    (p.Status == StatusPosudbe.Aktivna ||
                     p.Status == StatusPosudbe.Produzena ||
                     p.Status == StatusPosudbe.Kasni))
                .ToListAsync(cancellationToken);

            foreach (var loan in overdueLoans)
            {
                loan.Status = StatusPosudbe.Kasni;
                var reference = LoanReference(loan.IdPosudbe);

                var alreadySent = await _context.Obavjestenje.AnyAsync(
                    o => o.VrstaObavjestenja == VrstaObavjestenja.isticeRok &&
                         o.Poruka.Contains(reference),
                    cancellationToken);

                if (alreadySent || string.IsNullOrWhiteSpace(loan.Korisnik.Email))
                {
                    continue;
                }

                var body =
                    $"Poštovani/Poštovana {loan.Korisnik.Ime},\n\n" +
                    $"rok za vraćanje knjige \"{loan.Primjerak.Knjiga.Naslov}\" " +
                    $"istekao je {loan.RokVracanja:dd.MM.yyyy}. " +
                    "Molimo Vas da knjigu vratite u biblioteku u što kraćem roku.\n\n" +
                    "bibliotecha";

                if (await _emailService.SendAsync(
                        loan.Korisnik.Email,
                        "Opomena za prekoračenje roka vraćanja",
                        body,
                        cancellationToken))
                {
                    _context.Obavjestenje.Add(new Obavjestenje
                    {
                        KorisnikId = loan.KorisnikId,
                        Poruka = $"{body}\n{reference}",
                        DatumSlanja = today,
                        VrstaObavjestenja = VrstaObavjestenja.isticeRok
                    });
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task ExpireUnclaimedReservationsAsync(
            DateOnly today,
            CancellationToken cancellationToken)
        {
            var readyReservations = await _context.Rezervacija
                .Where(r => r.Status == StatusRezervacije.spremnaZaPreuzimanje)
                .ToListAsync(cancellationToken);

            foreach (var reservation in readyReservations)
            {
                var reference = ReservationReference(reservation.IdRezervacije);
                var notification = await _context.Obavjestenje
                    .Where(o =>
                        o.VrstaObavjestenja == VrstaObavjestenja.knjigaDostupna &&
                        o.Poruka.Contains(reference))
                    .OrderByDescending(o => o.DatumSlanja)
                    .FirstOrDefaultAsync(cancellationToken);

                if (notification == null)
                {
                    continue;
                }

                var pickupDeadline = BusinessDayCalculator.AddBusinessDays(
                    notification.DatumSlanja,
                    _settings.ReservationPickupBusinessDays);

                if (today <= pickupDeadline)
                {
                    continue;
                }

                reservation.Status = StatusRezervacije.Istekla;

                var reservedCopy = await _context.Primjerak
                    .FirstOrDefaultAsync(
                        p => p.KnjigaId == reservation.KnjigaId &&
                             p.Status == StatusPrimjerka.Rezervisan,
                        cancellationToken);

                if (reservedCopy != null)
                {
                    reservedCopy.Status = StatusPrimjerka.Dostupan;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task NotifyReservationQueueAsync(
            DateOnly today,
            CancellationToken cancellationToken)
        {
            var activeReservations = await _context.Rezervacija
                .Include(r => r.Korisnik)
                .Include(r => r.Knjiga)
                .Where(r => r.Status == StatusRezervacije.Aktivna)
                .OrderBy(r => r.KnjigaId)
                .ThenBy(r => r.PozicijaURedu)
                .ThenBy(r => r.DatumRezervacije)
                .ThenBy(r => r.IdRezervacije)
                .ToListAsync(cancellationToken);

            foreach (var reservation in activeReservations)
            {
                var availableCopy = await _context.Primjerak
                    .FirstOrDefaultAsync(
                        p => p.KnjigaId == reservation.KnjigaId &&
                             p.Status == StatusPrimjerka.Dostupan,
                        cancellationToken);

                if (availableCopy == null ||
                    string.IsNullOrWhiteSpace(reservation.Korisnik.Email))
                {
                    continue;
                }

                var reference = ReservationReference(reservation.IdRezervacije);
                var alreadySent = await _context.Obavjestenje.AnyAsync(
                    o => o.VrstaObavjestenja == VrstaObavjestenja.knjigaDostupna &&
                         o.Poruka.Contains(reference),
                    cancellationToken);

                if (alreadySent)
                {
                    continue;
                }

                var pickupDeadline = BusinessDayCalculator.AddBusinessDays(
                    today,
                    _settings.ReservationPickupBusinessDays);
                var body =
                    $"Poštovani/Poštovana {reservation.Korisnik.Ime},\n\n" +
                    $"knjiga \"{reservation.Knjiga.Naslov}\" sada je dostupna. " +
                    $"Rezervisani primjerak možete preuzeti do {pickupDeadline:dd.MM.yyyy}. " +
                    "Ako knjiga ne bude preuzeta u navedenom roku, bit će ponuđena " +
                    "sljedećem korisniku u redu rezervacija.\n\n" +
                    "bibliotecha";

                if (!await _emailService.SendAsync(
                        reservation.Korisnik.Email,
                        "Rezervisana knjiga je dostupna",
                        body,
                        cancellationToken))
                {
                    continue;
                }

                reservation.Status = StatusRezervacije.spremnaZaPreuzimanje;
                availableCopy.Status = StatusPrimjerka.Rezervisan;
                _context.Obavjestenje.Add(new Obavjestenje
                {
                    KorisnikId = reservation.KorisnikId,
                    Poruka = $"{body}\n{reference}",
                    DatumSlanja = today,
                    VrstaObavjestenja = VrstaObavjestenja.knjigaDostupna
                });

                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task ReorderActiveReservationsAsync(
            CancellationToken cancellationToken)
        {
            var reservations = await _context.Rezervacija
                .Where(r => r.Status == StatusRezervacije.Aktivna)
                .OrderBy(r => r.KnjigaId)
                .ThenBy(r => r.DatumRezervacije)
                .ThenBy(r => r.IdRezervacije)
                .ToListAsync(cancellationToken);

            foreach (var group in reservations.GroupBy(r => r.KnjigaId))
            {
                var position = 1;
                foreach (var reservation in group)
                {
                    reservation.PozicijaURedu = position++;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Periodična obrada obavještenja je završena.");
        }

        private static string LoanReference(int loanId) =>
            $"[POSUDBA:{loanId}]";

        private static string ReservationReference(int reservationId) =>
            $"[REZERVACIJA:{reservationId}]";
    }
}
