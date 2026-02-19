using ComicStoreASP.Data;

namespace ComicStoreASP.Services
{
    public class DatatableUpdateService(IServiceScopeFactory scopeFactory,
                                ILogger<DatatableUpdateService> Logs) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stopToken)
        {
            while (!stopToken.IsCancellationRequested)
            {
                await CheckForUpdates();

                await Task.Delay(TimeSpan.FromMinutes(1), stopToken);
            }
        }

        private async Task CheckForUpdates()
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }
    }
}
