// CleanroomEntryJob.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Api.Services;

namespace Api.Jobs
{
    public class CleanroomEntryJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public CleanroomEntryJob(IServiceProvider serviceProvider)
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
                    var service = scope.ServiceProvider.GetRequiredService<CleanroomEntryService>();
                    await service.ProcessAsync();
                    Console.WriteLine($"[CleanroomEntryJob] ✅ {DateTime.Now:HH:mm:ss}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CleanroomEntryJob] ❌ {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}