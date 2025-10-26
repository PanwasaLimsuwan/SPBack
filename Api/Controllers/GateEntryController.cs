using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FaceVectorController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FaceVectorController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetFaceVector(
            [FromQuery] string? division,
            [FromQuery] string? department,
            [FromQuery] string? section,
            [FromQuery] string? biz,
            [FromQuery] string? process,
            [FromQuery] DateTime? date
        )
        {
            var result = new List<object>();

            using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await conn.OpenAsync();

                // 1️⃣ ถ้าไม่เลือกวัน ให้ใช้วันล่าสุดใน FaceVector
                DateTime selectedDate;
                if (date.HasValue)
                {
                    selectedDate = date.Value;
                }
                else
                {
                    var latestDateCmd = new SqlCommand(
                        "SELECT TOP 1 [Timestamp] FROM FaceVector ORDER BY [Timestamp] DESC",
                        conn
                    );
                    var latestDateObj = await latestDateCmd.ExecuteScalarAsync();

                    if (latestDateObj == null || latestDateObj == DBNull.Value)
                    {
                        return Ok(result);
                    }

                    selectedDate = ((DateTime)latestDateObj).Date;
                }

                DateTime dateStart = selectedDate.Date;
                DateTime dateEnd = selectedDate.Date.AddDays(1).AddHours(7).AddSeconds(-1); // ครอบคลุมเวลาออกกะ 07:00 ของวันถัดไป

                // 2️⃣ Query หลัก: ดึงข้อมูล FaceVector ล่าสุดต่อพนักงาน
                var query = @"
                    WITH LatestFaceVector AS (
                        SELECT 
                            fv.FaceVectorID,
                            fv.EmpID,
                            fv.Vector,
                            fv.[Timestamp],
                            e.FirstName,
                            e.LastName,
                            e.Division,
                            e.Department,
                            e.Section,
                            e.Position,
                            e.Email,
                            e.ShiftCode,
                            e.Biz,
                            e.Process,
                            ROW_NUMBER() OVER (PARTITION BY fv.EmpID ORDER BY fv.[Timestamp] DESC) AS rn
                        FROM FaceVector fv
                        JOIN EmployeeInfo e ON fv.EmpID = e.EmpID
                        WHERE fv.[Timestamp] BETWEEN @dateStart AND @dateEnd
                        AND (@division IS NULL OR e.Division = @division)
                        AND (@department IS NULL OR e.Department = @department)
                        AND (@section IS NULL OR e.Section = @section)
                        AND (@biz IS NULL OR COALESCE(e.Biz, '') = @biz)
                        AND (@process IS NULL OR COALESCE(e.Process, '') = @process)
                    )
                    SELECT * FROM LatestFaceVector WHERE rn = 1
                    ORDER BY [Timestamp] DESC;
                ";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@division", (object?)division ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@department", (object?)department ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@section", (object?)section ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@biz", (object?)biz ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@process", (object?)process ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@dateStart", dateStart);
                    cmd.Parameters.AddWithValue("@dateEnd", dateEnd);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new
                            {
                                FaceVectorID = reader["FaceVectorID"],
                                EmpID = reader["EmpID"],
                                FirstName = reader["FirstName"]?.ToString(),
                                LastName = reader["LastName"]?.ToString(),
                                Division = reader["Division"]?.ToString(),
                                Department = reader["Department"]?.ToString(),
                                Section = reader["Section"]?.ToString(),
                                Position = reader["Position"]?.ToString(),
                                Email = reader["Email"]?.ToString(),
                                ShiftCode = reader["ShiftCode"]?.ToString(),
                                Biz = reader["Biz"]?.ToString(),
                                Process = reader["Process"]?.ToString(),
                                Timestamp = Convert.ToDateTime(reader["Timestamp"]).ToString("yyyy-MM-dd HH:mm:ss")
                            });
                        }
                    }
                }
            }

            return Ok(result);
        }
    }
}
