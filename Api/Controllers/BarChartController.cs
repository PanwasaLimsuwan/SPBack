using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BarChartController : LibResponseController
    {
        private readonly IConfiguration _configuration;

        public BarChartController(IConfiguration configuration) : base(configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetBarChartData()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            string query = @"
                SELECT 
                    e.Process, 
                    e.SkillGroup, 
                    e.Require
                FROM ManpowerReq e
                ORDER BY e.Process, e.SkillGroup;
            ";

            List<object> resultList = new List<object>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();

                        while (await reader.ReadAsync())
                        {
                            var result = new
                            {
                                process = reader["Process"],
                                skillGroup = reader["SkillGroup"],
                                require = reader["Require"]
                            };
                            resultList.Add(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError("Database error: " + ex.Message);
            }

            if (resultList.Count == 0)
            {
                return NotFound("No data found.");
            }

            return Success(new JsonResult(resultList));
        }
    }
}
