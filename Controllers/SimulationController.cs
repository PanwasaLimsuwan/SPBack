using Microsoft.AspNetCore.Mvc;
using Api.Services;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/simulation")]
    public class SimulationController : ControllerBase
    {
        private readonly DailySimulationService _simulationService;

        public SimulationController(DailySimulationService simulationService)
        {
            _simulationService = simulationService;
        }

        [HttpPost("run")]
        public async Task<IActionResult> Run()
        {
            await _simulationService.RunNextDayAsync();
            return Ok("Next day simulated successfully.");
        }
    }
}