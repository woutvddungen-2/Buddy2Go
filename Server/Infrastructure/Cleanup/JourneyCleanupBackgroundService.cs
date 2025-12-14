namespace Server.Infrastructure.Cleanup
{
    public class JourneyCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<JourneyCleanupBackgroundService> logger;

        public JourneyCleanupBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<JourneyCleanupBackgroundService> logger)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Run 6 hourly.
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using IServiceScope scope = scopeFactory.CreateScope();
                    JourneyCleanupService cleanup = scope.ServiceProvider.GetRequiredService<JourneyCleanupService>();

                    await cleanup.CleanupOldJourneysAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Background journey cleanup loop failed.");
                }

                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }

}
