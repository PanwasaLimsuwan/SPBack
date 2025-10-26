using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OJTandInspectionSkillController : ControllerBase
    {
        private readonly string _connectionString;

        public OJTandInspectionSkillController(IConfiguration configuration)
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
            var result = new List<object>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query =
                    @"
        SELECT 
            o.CourseNo,
            o.CourseGroup,
            o.Biz,
            o.Process,
            o.CerNo,
            o.Active,
            o.SkillGroup,
            o.EmpID,
            e.Division,
            e.Department,
            e.Section,
            e.Biz AS EmployeeBiz,
            e.Process AS EmployeeProcess
        FROM OJTandInspectionSkill o
        LEFT JOIN EmployeeInfo e ON o.EmpID = e.EmpID
        WHERE 1 = 1";

                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    // ปรับกรองค่าใน SQL Query
                    if (!string.IsNullOrEmpty(division))
                    {
                        query += " AND e.Division = @division";
                        cmd.Parameters.AddWithValue("@division", division);
                    }

                    if (!string.IsNullOrEmpty(department))
                    {
                        query += " AND e.Department = @department";
                        cmd.Parameters.AddWithValue("@department", department);
                    }

                    if (!string.IsNullOrEmpty(section))
                    {
                        query += " AND e.Section = @section";
                        cmd.Parameters.AddWithValue("@section", section);
                    }

                    if (!string.IsNullOrEmpty(biz))
                    {
                        query += " AND e.Biz = @biz";
                        cmd.Parameters.AddWithValue("@biz", biz);
                    }

                    if (!string.IsNullOrEmpty(process))
                    {
                        query += " AND e.Process = @process";
                        cmd.Parameters.AddWithValue("@process", process);
                    }

                    cmd.CommandText = query;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(
                                new
                                {
                                    courseNo = reader["CourseNo"]?.ToString(),
                                    courseGroup = reader["CourseGroup"]?.ToString(),
                                    biz = reader["Biz"]?.ToString(),
                                    process = reader["Process"]?.ToString(),
                                    cerNo = reader["CerNo"]?.ToString(),
                                    active = reader["Active"] == DBNull.Value
                                        ? null
                                        : (int?)reader["Active"],
                                    skillGroup = reader["SkillGroup"]?.ToString(),
                                    EmpID = reader["EmpID"] == DBNull.Value
                                        ? (int?)null
                                        : Convert.ToInt32(reader["EmpID"]),
                                    division = reader["Division"]?.ToString(),
                                    department = reader["Department"]?.ToString(),
                                    section = reader["Section"]?.ToString(),
                                    employeeBiz = reader["EmployeeBiz"]?.ToString(),
                                    employeeProcess = reader["EmployeeProcess"]?.ToString(),
                                }
                            );
                        }
                    }
                }
            }

            return Ok(result);
        }
    }
}
