using Microsoft.Extensions.Options;

namespace bibliotecha.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly NotificationSettings _settings;
        private readonly ILogger<NotificationBackgroundService> _logger;

        public NotificationBackgroundService(
            IServiceScopeFactory scopeFactory,
            IOptions<NotificationSettings> settings,
            ILogger<NotificationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _settings = settings.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_settings.Enabled)
            {
                _logger.LogInformation("Periodična obrada obavještenja je isključena.");
                return;
            }

            var interval = TimeSpan.FromMinutes(
                Math.Max(1, _settings.CheckIntervalMinutes));

            await ProcessAsync(stoppingToken);

            using var timer = new PeriodicTimer(interval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessAsync(stoppingToken);
            }
        }

        private async Task ProcessAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var processor =
                    scope.ServiceProvider.GetRequiredService<NotificationProcessor>();
                await processor.ProcessAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Došlo je do greške tokom periodične obrade obavještenja.");
            }
        }
    }
}
