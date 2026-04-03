// ManpowerPlanJob.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Api.Services;

namespace Api.Jobs
{
    public class ManpowerPlanJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ManpowerPlanJob(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<ManpowerPlanService>();
                    await service.UpdateActualHeadcountAsync();
                    Console.WriteLine($"[ManpowerPlanJob] ✅ {DateTime.Now:HH:mm:ss}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ManpowerPlanJob] ❌ {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}