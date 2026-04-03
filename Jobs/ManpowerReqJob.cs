using Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Api.Jobs
{
    public class ManpowerReqJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ManpowerReqJob(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        // {
        //     while (!stoppingToken.IsCancellationRequested)
        //     {
        //         try
        //         {
        //             using var scope = _serviceProvider.CreateScope();
        //             var service = scope.ServiceProvider
        //                 .GetRequiredService<ManpowerReqService>();

        //             await service.RecalculateAsync();  // 🔥 แก้ชื่อ method
        //             Console.WriteLine($"[ManpowerReqJob] ✅ {DateTime.Now:HH:mm:ss}");
        //         }
        //         catch (Exception ex)
        //         {
        //             Console.WriteLine($"[ManpowerReqJob] ❌ {ex.Message}");
        //         }

        //         await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        //     }
        // }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 🔥 รอ 4 นาที ให้ Attendance และ ManpowerPlan รันก่อน
            await Task.Delay(TimeSpan.FromMinutes(4), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<ManpowerReqService>();

                    await service.RecalculateTodayAsync(); // 🔥 เปลี่ยนจาก RecalculateAsync
                    Console.WriteLine($"[ManpowerReqJob] ✅ {DateTime.Now:HH:mm:ss}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ManpowerReqJob] ❌ {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
