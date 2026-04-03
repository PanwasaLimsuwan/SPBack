// WorktimeJob.cs
using Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Api.Jobs
{
    public class WorktimeJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public WorktimeJob(IServiceProvider serviceProvider)
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

                    var worktimeService =
                        scope.ServiceProvider.GetRequiredService<WorktimeService>();
                    await worktimeService.CalculateAsync();

                    var otService = scope.ServiceProvider.GetRequiredService<OTService>();
                    await otService.ProcessAsync(); // 🔥 สำคัญมาก

                    Console.WriteLine($"[Worktime+OT] ✅ {DateTime.Now:HH:mm:ss}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Worktime+OT] ❌ {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
