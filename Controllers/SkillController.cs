using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SkillController : ControllerBase
    {
        private readonly string _connectionString;

        public SkillController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? division,
            [FromQuery] string? department,
            [FromQuery] string? section,
            [FromQuery] string? biz,
            [FromQuery] string? process
        )
        {
            var skills = new List<object>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT 
                        s.EmpID,
                        s.FirstName,
                        s.LastName,
                        s.Material,
                        s.Operation,
                        s.MachineSAB1,
                        s.MachineSAB2,
                        s.MachineSAB3,
                        s.Inspection,
                        e.Division,
                        e.Department,
                        e.Section,
                        e.Biz,
                        e.Process AS Process
                    FROM Skill s
                    LEFT JOIN EmployeeInfo e ON s.EmpID = e.EmpID
                    WHERE 1=1";

                var parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(division))
                {
                    query += " AND (e.Division = @Division OR e.Division IS NULL)";
                    parameters.Add(new SqlParameter("@Division", division));
                }

                if (!string.IsNullOrEmpty(department))
                {
                    query += " AND (e.Department = @Department OR e.Department IS NULL)";
                    parameters.Add(new SqlParameter("@Department", department));
                }

                if (!string.IsNullOrEmpty(section))
                {
                    query += " AND (e.Section = @Section OR e.Section IS NULL)";
                    parameters.Add(new SqlParameter("@Section", section));
                }

                if (!string.IsNullOrEmpty(biz))
                {
                    query += " AND (e.Biz = @Biz OR e.Biz IS NULL)";
                    parameters.Add(new SqlParameter("@Biz", biz));
                }

                if (!string.IsNullOrEmpty(process))
                {
                    query += " AND (e.Process = @Process OR e.Process IS NULL)";
                    parameters.Add(new SqlParameter("@Process", process));
                }

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddRange(parameters.ToArray());

                    var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        // ✅ Mapping SkillGroup
                        string skillGroup;
                        var skillLevels = new Dictionary<string, int>
                        {
                            { "Material", (int)reader["Material"] },
                            { "Operation", (int)reader["Operation"] },
                            { "MachineSAB1", (int)reader["MachineSAB1"] },
                            { "MachineSAB2", (int)reader["MachineSAB2"] },
                            { "MachineSAB3", (int)reader["MachineSAB3"] },
                            { "Inspection", (int)reader["Inspection"] }
                        };

                        skillGroup = skillLevels.OrderByDescending(s => s.Value).First().Key;

                        skills.Add(new
                        {
                            EmpID = Convert.ToInt32(reader["EmpID"]),
                            firstName = reader["FirstName"]?.ToString(),
                            lastName = reader["LastName"]?.ToString(),
                            material = (int)reader["Material"],
                            operation = (int)reader["Operation"],
                            machineSAB1 = (int)reader["MachineSAB1"],
                            machineSAB2 = (int)reader["MachineSAB2"],
                            machineSAB3 = (int)reader["MachineSAB3"],
                            inspection = (int)reader["Inspection"],
                            division = reader["Division"]?.ToString(),
                            department = reader["Department"]?.ToString(),
                            section = reader["Section"]?.ToString(),
                            biz = reader["Biz"]?.ToString(),
                            process = reader["Process"]?.ToString(),
                            skillGroup = skillGroup
                        });
                    }
                }
            }

            return Ok(skills);
        }
    }
}
