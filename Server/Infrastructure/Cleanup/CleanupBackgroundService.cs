namespace Server.Infrastructure.Cleanup
{
    public class CleanupBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<CleanupBackgroundService> logger;

        public CleanupBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<CleanupBackgroundService> logger)
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
                    DangerousPlaceCleanupService DPcleanup = scope.ServiceProvider.GetRequiredService<DangerousPlaceCleanupService>();
                    JourneyCleanupService Journeycleanup = scope.ServiceProvider.GetRequiredService<JourneyCleanupService>();

                    await Journeycleanup.Cleanup(stoppingToken);
                    await DPcleanup.Cleanup(stoppingToken);  
                    
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
