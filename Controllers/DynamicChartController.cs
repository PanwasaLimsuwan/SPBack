// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Data.SqlClient;
// using System;
// using System.Collections.Generic;
// using System.Data;
// using System.Linq;
// using System.Text.Json;
// using System.Threading.Tasks;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api")]
//     public class DynamicChartController : ControllerBase
//     {
//         private readonly string _connectionString;

//         // ✅ รายชื่อตารางที่อนุญาต (ป้องกัน SQL Injection)
//         private static readonly HashSet<string> AllowedTables = new HashSet<string>
//         {
//             "Assignment", "Attendance", "Cameras", "EICC_Control",
//             "OJTandInspectionSkill", "HeadcountTransition", "ManpowerPlan",
//             "ManpowerReq", "Skill", "Transactions", "Worktime", "EmployeeInfo",
//             "CalculatedWorktime"
//         };

//         public DynamicChartController(IConfiguration configuration)
//         {
//             _connectionString = configuration.GetConnectionString("DefaultConnection");
//         }

//         // ✅ GET: api/chart-metadata - ดึง metadata ของตารางทั้งหมด
//         [HttpGet("chart-metadata")]
//         public async Task<IActionResult> GetChartMetadata()
//         {
//             try
//             {
//                 using var conn = new SqlConnection(_connectionString);
//                 await conn.OpenAsync();

//                 var query = @"
//                     SELECT 
//                         t.TABLE_NAME,
//                         c.COLUMN_NAME,
//                         c.DATA_TYPE
//                     FROM INFORMATION_SCHEMA.TABLES t
//                     JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME
//                     WHERE t.TABLE_SCHEMA = 'dbo'
//                     AND t.TABLE_TYPE = 'BASE TABLE'
//                     AND t.TABLE_NAME IN (
//                         'Assignment', 'Attendance', 'Cameras', 'EICC_Control',
//                         'OJTandInspectionSkill', 'HeadcountTransition', 'ManpowerPlan',
//                         'ManpowerReq', 'Skill', 'Transactions', 'Worktime', 'EmployeeInfo',
//                         'CalculatedWorktime'
//                     )
//                     ORDER BY t.TABLE_NAME, c.ORDINAL_POSITION";

//                 using var cmd = new SqlCommand(query, conn);
//                 var metadata = new Dictionary<string, List<ColumnInfo>>();

//                 using var reader = await cmd.ExecuteReaderAsync();
//                 while (await reader.ReadAsync())
//                 {
//                     var tableName = reader.GetString(0);
//                     var columnName = reader.GetString(1);
//                     var dataType = reader.GetString(2);

//                     if (!metadata.ContainsKey(tableName))
//                     {
//                         metadata[tableName] = new List<ColumnInfo>();
//                     }

//                     metadata[tableName].Add(new ColumnInfo
//                     {
//                         Name = columnName,
//                         Type = dataType
//                     });
//                 }

//                 Console.WriteLine($"✅ Metadata loaded: {metadata.Count} tables");
//                 return Ok(new { success = true, metadata });
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"❌ Error loading metadata: {ex.Message}");
//                 return StatusCode(500, new { error = ex.Message });
//             }
//         }

//         // ✅ POST: api/dynamic-chart - สร้างกราฟ
//         [HttpPost("dynamic-chart")]
//         public async Task<IActionResult> GenerateDynamicChart([FromBody] DynamicChartRequest request)
//         {
//             try
//             {
//                 // Validate table name
//                 if (!AllowedTables.Contains(request.TableName))
//                 {
//                     return BadRequest(new { error = "Invalid table name" });
//                 }

//                 // Validate required fields
//                 if (string.IsNullOrEmpty(request.XAxis) || request.YAxis == null || request.YAxis.Length == 0)
//                 {
//                     return BadRequest(new { error = "XAxis and at least one YAxis are required" });
//                 }

//                 using var conn = new SqlConnection(_connectionString);
//                 await conn.OpenAsync();

//                 // ✅ สร้าง SQL Query แบบ Dynamic
//                 var query = BuildDynamicQuery(request);
//                 Console.WriteLine($"📊 Generated Query: {query}");

//                 using var cmd = new SqlCommand(query, conn);
//                 AddParameters(cmd, request);

//                 var results = new List<Dictionary<string, object>>();
//                 using var reader = await cmd.ExecuteReaderAsync();
                
//                 while (await reader.ReadAsync())
//                 {
//                     var row = new Dictionary<string, object>();
//                     for (int i = 0; i < reader.FieldCount; i++)
//                     {
//                         var columnName = reader.GetName(i);
//                         var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        
//                         // ✅ แปลงค่าเป็น type ที่เหมาะสม
//                         if (value is decimal decimalValue)
//                         {
//                             row[columnName] = (double)decimalValue;
//                         }
//                         else if (value is DateTime dateValue)
//                         {
//                             row[columnName] = dateValue.ToString("yyyy-MM-dd");
//                         }
//                         else
//                         {
//                             row[columnName] = value;
//                         }
//                     }
//                     results.Add(row);
//                 }

//                 Console.WriteLine($"✅ Query returned {results.Count} rows");
//                 return Ok(new
//                 {
//                     success = true,
//                     data = results,
//                     chartType = request.ChartType,
//                     xAxisLabel = request.XAxis,
//                     yAxisLabel = string.Join(", ", request.YAxis)
//                 });
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"❌ Error: {ex.Message}");
//                 return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
//             }
//         }

//         // ✅ สร้าง SQL Query แบบ Dynamic
//         private string BuildDynamicQuery(DynamicChartRequest request)
//         {
//             var selectFields = new List<string> { request.XAxis };

//             // เพิ่ม Y-Axis fields พร้อม aggregation
//             if (!string.IsNullOrEmpty(request.Aggregation))
//             {
//                 foreach (var yField in request.YAxis)
//                 {
//                     selectFields.Add($"{request.Aggregation}({yField}) AS {yField}");
//                 }
//             }
//             else
//             {
//                 selectFields.AddRange(request.YAxis);
//             }

//             var query = $"SELECT {string.Join(", ", selectFields)} FROM {request.TableName}";

//             // ✅ เพิ่ม WHERE clause
//             var whereClauses = new List<string>();

//             if (request.Filters != null)
//             {
//                 foreach (var filter in request.Filters)
//                 {
//                     if (!string.IsNullOrEmpty(filter.Value) && filter.Value != "ALL")
//                     {
//                         whereClauses.Add($"{filter.Key} = @{filter.Key}");
//                     }
//                 }
//             }

//             // Date Range Filter
//             if (request.DateRange != null && 
//                 !string.IsNullOrEmpty(request.DateRange.Start) && 
//                 !string.IsNullOrEmpty(request.DateRange.End))
//             {
//                 whereClauses.Add($"{request.XAxis} BETWEEN @StartDate AND @EndDate");
//             }

//             if (whereClauses.Count > 0)
//             {
//                 query += $" WHERE {string.Join(" AND ", whereClauses)}";
//             }

//             // ✅ เพิ่ม GROUP BY
//             if (!string.IsNullOrEmpty(request.Aggregation))
//             {
//                 query += $" GROUP BY {request.XAxis}";
//             }

//             // ✅ เพิ่ม ORDER BY
//             query += $" ORDER BY {request.XAxis}";

//             return query;
//         }

//         // ✅ เพิ่ม Parameters ใน SQL Command
//         private void AddParameters(SqlCommand cmd, DynamicChartRequest request)
//         {
//             if (request.Filters != null)
//             {
//                 foreach (var filter in request.Filters)
//                 {
//                     if (!string.IsNullOrEmpty(filter.Value) && filter.Value != "ALL")
//                     {
//                         cmd.Parameters.AddWithValue($"@{filter.Key}", filter.Value);
//                     }
//                 }
//             }

//             if (request.DateRange != null && 
//                 !string.IsNullOrEmpty(request.DateRange.Start) && 
//                 !string.IsNullOrEmpty(request.DateRange.End))
//             {
//                 cmd.Parameters.AddWithValue("@StartDate", DateTime.Parse(request.DateRange.Start));
//                 cmd.Parameters.AddWithValue("@EndDate", DateTime.Parse(request.DateRange.End));
//             }
//         }
//     }

//     // ✅ DTOs
//     public class DynamicChartRequest
//     {
//         public string TableName { get; set; }
//         public string XAxis { get; set; }
//         public string[] YAxis { get; set; }
//         public string Aggregation { get; set; } // SUM, COUNT, AVG, MIN, MAX
//         public string ChartType { get; set; } // bar, line, area, pie
//         public Dictionary<string, string> Filters { get; set; }
//         public DateRangeFilter DateRange { get; set; }
//     }

//     public class DateRangeFilter
//     {
//         public string Start { get; set; }
//         public string End { get; set; }
//     }

//     public class ColumnInfo
//     {
//         public string Name { get; set; }
//         public string Type { get; set; }
//     }
// }