using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Api.Models;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EICCControlController : ControllerBase
    {
        private readonly string _connectionString;

        public EICCControlController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var results = new List<object>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT 
                        eicc.ControlID, eicc.EmpID, eicc.WeekID, eicc.TotalHours, eicc.DaysWorked, eicc.TotalOT, eicc.Status,
                        ei.Division, ei.Department, ei.Section, ei.Biz, ei.Process
                    FROM EICC_Control eicc
                    JOIN EmployeeInfo ei ON eicc.EmpID = CAST(ei.EmpID AS VARCHAR)";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        results.Add(new
                        {
                            controlID = reader.GetInt32(0),
                            empID = reader["EmpID"].ToString(),
                            weekID = reader.GetInt32(2),
                            totalHours = reader.IsDBNull(3) ? null : (float?)reader.GetFloat(3),
                            daysWorked = reader.IsDBNull(4) ? null : (int?)reader.GetInt32(4),
                            totalOT = reader.IsDBNull(5) ? null : (float?)reader.GetDouble(5),
                            status = reader["Status"]?.ToString(),
                            division = reader["Division"]?.ToString(),
                            department = reader["Department"]?.ToString(),
                            section = reader["Section"]?.ToString(),
                            biz = reader["Biz"]?.ToString(),
                            process = reader["Process"]?.ToString()
                        });
                    }
                }
            }

            return Ok(results);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EICC_Control control)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand(@"
                    INSERT INTO EICC_Control (EmpID, WeekID, TotalHours, DaysWorked, TotalOT, Status)
                    VALUES (@EmpID, @WeekID, @TotalHours, @DaysWorked, @TotalOT, @Status)", conn);

                cmd.Parameters.AddWithValue("@EmpID", control.EmpID);
                cmd.Parameters.AddWithValue("@WeekID", control.WeekID);
                cmd.Parameters.AddWithValue("@TotalHours", (object)control.TotalHours ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DaysWorked", (object)control.DaysWorked ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TotalOT", (object)control.TotalOT ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", control.Status);

                await cmd.ExecuteNonQueryAsync();
            }

            return Ok("Created");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, EICC_Control control)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand(@"
                    UPDATE EICC_Control SET
                        EmpID = @EmpID,
                        WeekID = @WeekID,
                        TotalHours = @TotalHours,
                        DaysWorked = @DaysWorked,
                        TotalOT = @TotalOT,
                        Status = @Status
                    WHERE ControlID = @ControlID", conn);

                cmd.Parameters.AddWithValue("@ControlID", id);
                cmd.Parameters.AddWithValue("@EmpID", control.EmpID);
                cmd.Parameters.AddWithValue("@WeekID", control.WeekID);
                cmd.Parameters.AddWithValue("@TotalHours", (object)control.TotalHours ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DaysWorked", (object)control.DaysWorked ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TotalOT", (object)control.TotalOT ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", control.Status);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand("DELETE FROM EICC_Control WHERE ControlID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    return NotFound();
            }

            return NoContent();
        }
    }
}
