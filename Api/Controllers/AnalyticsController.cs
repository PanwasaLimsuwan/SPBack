using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly AnalyticsRepository _repo;

        public AnalyticsController(AnalyticsRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        [HttpGet("ping")]
        public IActionResult Ping() => Ok(new { ok = true });

        // สำหรับการ์ด Insights: POST /api/Analytics/run
        [HttpPost("run")]
        public async Task<IActionResult> Run([FromBody] InsightRequest body)
        {
            if (body == null) return BadRequest("Body is required.");

            object result;
            switch (body.key)
            {
                case "ot-hotspots":
                    result = new
                    {
                        title = $"OT Hotspots (last {body.Weeks ?? 4} weeks)",
                        data = await _repo.OT_Hotspots(body)
                    };
                    break;

                case "headcount-vs-plan":
                    result = new
                    {
                        title = $"Headcount vs Plan ({body.Year ?? DateTime.Now.Year})",
                        data = await _repo.HeadcountVsPlanVariance(body)
                    };
                    break;

                case "absence-streaks":
                    result = new
                    {
                        title = $"Consecutive absence (>=2 days) {body.Year ?? DateTime.Now.Year}-{body.Month ?? DateTime.Now.Month}",
                        data = await _repo.AbsenceStreaks(body)
                    };
                    break;

                case "eicc-risk":
                    result = new
                    {
                        title = $"EICC risk (hours) {body.Year ?? DateTime.Now.Year}-{body.Month ?? DateTime.Now.Month}",
                        data = await _repo.EICC_Risk(body)
                    };
                    break;

                case "skill-gaps":
                default:
                    result = new
                    {
                        title = "Skill gaps by process",
                        data = await _repo.SkillGapByProcess(body)
                    };
                    break;
            }

            return Ok(result);
        }

        // ถ้าอยากเก็บ endpoint GET แบบเดิมไว้ด้วยก็ได้:
        // GET /api/Analytics/headcount-vs-plan?year=2025&process=ASSY1
        [HttpGet("headcount-vs-plan")]
        public async Task<IActionResult> GetHeadcountVsPlan([FromQuery] int year, [FromQuery] string? process)
        {
            var req = new InsightRequest { Year = year == 0 ? DateTime.Now.Year : year, Process = process ?? "ALL" };
            var data = await _repo.HeadcountVsPlanVariance(req);
            return Ok(data);
        }
    }
}
