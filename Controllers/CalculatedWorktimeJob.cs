using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Api.Jobs
{
    public class CalculatedWorktimeJob : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CalculatedWorktimeJob(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    // var response = await client.PostAsync("https://databasemanpowerdb-cpbbfhaca3fchbgr.southeastasia-01.azurewebsites.net/api/GateToWorktime", null);
                    // var response = await client.PostAsync("https://deploymanpowerdb-f5a0h6fqaehdajck.southeastasia-01.azurewebsites.net/api/GateToWorktime", null);
                    // var response = await client.PostAsync("https://databasemanpowerdb.database.windows.net/api/GateToWorktime", null);
                    var response = await client.PostAsync("http://localhost:5000/api/GateToWorktime", null);
                    Console.WriteLine($"[CalculatedWorktimeJob] Response: {response.StatusCode} @ {DateTime.Now}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CalculatedWorktimeJob] ERROR: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // ✅ ทุก 5 วินาที
            }
        }
    }
}
