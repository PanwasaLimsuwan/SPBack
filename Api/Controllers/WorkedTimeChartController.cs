using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkedTimeChartController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public WorkedTimeChartController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetWorkedTimeData()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            string query = @"
                SELECT 
                    e.EmpID, 
                    e.Firstname, 
                    e.Lastname, 
                    SUM(t.TotalHours) AS TotalWorkedHours
                FROM EICC_Control t
                JOIN EmployeeInfo e ON e.EmpID = t.EmpID
                GROUP BY e.EmpID, e.Firstname, e.Lastname
                HAVING SUM(t.TotalHours) > 0
                ORDER BY e.EmpID;
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
                                
                                firstname = reader["Firstname"],
                                lastname = reader["Lastname"],
                                workTime = reader["TotalWorkedHours"]
                            };
                            resultList.Add(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

            if (resultList.Count == 0)
            {
                return NotFound("No data found.");
            }

            return Ok(resultList);
        }
    }
}
