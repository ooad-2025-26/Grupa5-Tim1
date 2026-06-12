using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace bibliotecha.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(
            IOptions<EmailSettings> settings,
            ILogger<SmtpEmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<bool> SendAsync(
            string recipient,
            string subject,
            string body,
            CancellationToken cancellationToken = default)
        {
            if (!_settings.Enabled)
            {
                _logger.LogWarning(
                    "Email nije poslan korisniku {Recipient} jer SMTP slanje nije uključeno.",
                    recipient);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_settings.Host) ||
                string.IsNullOrWhiteSpace(_settings.FromAddress))
            {
                _logger.LogError("SMTP postavke nisu potpune.");
                return false;
            }

            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.FromAddress, _settings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                message.To.Add(recipient);

                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    EnableSsl = _settings.UseSsl
                };

                if (!string.IsNullOrWhiteSpace(_settings.Username))
                {
                    client.Credentials = new NetworkCredential(
                        _settings.Username,
                        _settings.Password);
                }

                cancellationToken.ThrowIfCancellationRequested();
                await client.SendMailAsync(message, cancellationToken);
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Slanje emaila korisniku {Recipient} nije uspjelo.",
                    recipient);
                return false;
            }
        }
    }
}
