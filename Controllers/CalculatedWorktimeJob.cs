// using System;
// using System.Net.Http;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Hosting;

// public class CalculatedWorktimeJob : BackgroundService
// {
//     private readonly IHttpClientFactory _httpClientFactory;

//     public CalculatedWorktimeJob(IHttpClientFactory httpClientFactory)
//     {
//         _httpClientFactory = httpClientFactory;
//     }

//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         while (!stoppingToken.IsCancellationRequested)
//         {
//             try
//             {
//                 var client = _httpClientFactory.CreateClient();
//                 var response = await client.PostAsync("http://localhost:5000/api/GateToWorktime", null);
//                 Console.WriteLine($"[CalculatedWorktimeJob] Response: {response.StatusCode} @ {DateTime.Now}");
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"[CalculatedWorktimeJob] ERROR: {ex.Message}");
//             }

//             await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
//         }
//     }
// }
