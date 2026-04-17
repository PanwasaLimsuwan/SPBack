using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TransactionSimulatorJob : ControllerBase
{
    private readonly TransactionSimulatorService _simulator;
    private readonly IConfiguration _configuration;

    public TransactionSimulatorJob(
        TransactionSimulatorService simulator,
        IConfiguration configuration)
    {
        _simulator = simulator;
        _configuration = configuration;
    }

    // POST: api/TransactionSimulatorJob/trigger
    [HttpPost("trigger")]
    public async Task<IActionResult> TriggerManual()
    {
        try
        {
            var connStr = _configuration.GetConnectionString("DefaultConnection");
            await _simulator.RunOnceAsync(connStr);
            return Ok("✅ Simulation triggered manually.");
        }
        catch (Exception ex)
        {
            return Problem(title: "Simulation failed", detail: ex.Message, statusCode: 500);
        }
    }

    [HttpPost("simulate-end-shift")]
public async Task<IActionResult> SimulateEndShift([FromQuery] string shift = "NIGHT")
{
    try
    {
        var connStr = _configuration.GetConnectionString("DefaultConnection");
        await _simulator.SimulateEndShiftAsync(connStr, shift);
        return Ok($"✅ Simulate end of {shift} shift สำเร็จ");
    }
    catch (Exception ex)
    {
        return Problem(title: "Simulation failed", detail: ex.Message, statusCode: 500);
    }
}
}