using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

// ResponseController version 1.0.2
namespace API_ProductionQuality.Controllers
{
    /// <summary>
    ///     Controller for executing SQL queries and returning JSON results.
    /// </summary>
    /// <remarks>
    ///     Controller สำหรับการดำเนินการคำสั่ง SQL และการส่งผลลัพธ์เป็น JSON
    /// </remarks>
    public class LibResponseController : Controller
    {
        /// <summary>
        ///     Returns standardised JSON response with status, code, and data that has been defined.
        /// </summary>
        /// <remarks>
        ///     ส่งคำตอบ JSON มาตรฐานที่มีสถานะ รหัส และข้อมูลที่ถูกกำหนดไว้
        /// </remarks>
        /// <param name="status">(required) The status of the response. (Example: "success", "error")</param>
        /// <param name="code">(required) The code of the response. (Example: 200, 404)</param>
        /// <param name="message">(optional) The message of the response (Example: "Data retrieved successfully.")</param>
        /// <param name="data">(optional) The data in <see cref="JsonResult"/> format.</param>
        /// <returns>The standardised JSON response.</returns>
        public JsonResult ResponseResult(
            string status,
            int code,
            string message = "",
            JsonResult data = null
        )
        {
            if (data == null)
            {
                return new JsonResult(
                    new
                    {
                        status = status,
                        code = code,
                        message = message,
                        time = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        data = new { },
                    }
                )
                {
                    StatusCode = code,
                };
            }
            else
            {
                // แก้กรณี data is a JsonResult
                if (data is JsonResult jsonResult)
                {
                    return new JsonResult(
                        new
                        {
                            status = status,
                            code = code,
                            message = message,
                            time = DateTimeOffset.Now.ToUnixTimeSeconds(),
                            data = data.Value,
                        }
                    )
                    {
                        StatusCode = code,
                    };
                }
                else
                {
                    return new JsonResult(
                        new
                        {
                            status = status,
                            code = code,
                            message = message,
                            time = DateTimeOffset.Now.ToUnixTimeSeconds(),
                            data = data,
                        }
                    )
                    {
                        StatusCode = code,
                    };
                }
            }
        }

        /// <summary>
        ///    Returns standardised JSON response reference from the <see cref="JsonResult"/> object.
        /// </summary>
        /// <remarks>
        ///     ส่งคำตอบ JSON มาตรฐานที่อ้างอิงจากวัตถุ หรือ Success หากเป็นข้อมูล
        /// </remarks>
        /// <param name="data">(required) The data in <see cref="JsonResult"/> format.</param>
        /// <returns>The standardised JSON response.</returns>
        public JsonResult Result(
            JsonResult data,
            string message = "Data retrieved successfully.",
            bool isPost = false
        )
        {
            // Deserialize the JsonResult object to a JsonElement
            var jsonData = JsonSerializer.Serialize(data.Value);
            var dynamicData = JsonSerializer.Deserialize<JsonElement>(jsonData);

            // If type is not an object, return the data
            if (dynamicData.ValueKind != JsonValueKind.Object)
            {
                return Success(data, message);
            }
            else
            {
                // Check if the JsonElement contains the standardized fields
                if (
                    dynamicData.TryGetProperty("status", out JsonElement status)
                    && dynamicData.TryGetProperty("code", out JsonElement code)
                )
                {
                    return data;
                }
                else
                {
                    if (isPost)
                    {
                        return Created(data, message);
                    }
                    else
                    {
                        return Success(data, message);
                    }
                }
            }
        }

        /// <summary>
        ///     Returns standardised JSON response for a successful POST request.
        /// </summary>
        /// <remarks>
        ///     ส่งคำตอบ JSON มาตรฐานที่อ้างอิงจากวัตถุ หรือ Created หากเป็นข้อมูล
        /// </remarks>
        /// <param name="data">(required) The data in <see cref="JsonResult"/> format.</param>
        /// <returns>The standardised JSON response.</returns>
        public JsonResult ResultPOST(JsonResult data, string message = "Data created successfully.")
        {
            return Result(data, message, true);
        }

        /// <summary>
        ///     Returns standardised JSON response for a successful POST request.
        /// </summary>
        /// <remarks>
        ///     ส่งคำตอบ JSON มาตรฐานที่อ้างอิงจากวัตถุ หรือ Updated หากเป็นข้อมูล
        /// </remarks>
        /// <param name="data">(required) The data in <see cref="JsonResult"/> format.</param>
        /// <returns>The standardised JSON response.</returns>
        public JsonResult ResultPUT(JsonResult data, string message = "Data updated successfully.")
        {
            return ResponseResult("success", 201, "Data updated successfully.", data);
        }

        /// <summary>
        ///     Returns standardised JSON response for a successful request.
        /// </summary>
        /// <remarks>
        ///     ส่งคำตอบ JSON มาตรฐานสำหรับคำขอที่ประสบความสำเร็จ (ดึงข้อมูลสำเร็จ)
        /// </remarks>
        /// <param name="data">(required) The data in <see cref="JsonResult"/> format.</param>
        /// <returns>The standardised JSON response.</returns>
        public JsonResult Success(JsonResult data)
        {
            return ResponseResult("success", 200, "Data retrieved successfully.", data);
        }

        private JsonResult Success(JsonResult data, string message)
        {
            return ResponseResult("success", 200, message, data);
        }

        private JsonResult Success(string message)
        {
            return ResponseResult("success", 200, message);
        }

        /// <summary>
        ///     Returns standardised JSON response for a created request.
        /// </summary>
        /// <remarks>
        ///     ส่งคำตอบ JSON มาตรฐานสำหรับคำขอที่สร้างขึ้นสำเร็จ/แก้ไขข้อมูลสำเร็จ (สร้างข้อมูลใหม่ หรือแก้ไขข้อมูลเดิม)
        /// </remarks>
        /// <param name="data">(required) The data in <see cref="JsonResult"/> format.</param>
        /// <returns>The standardised JSON response.</returns>
        public JsonResult Created(JsonResult data)
        {
            return ResponseResult("success", 201, "Data created successfully.", data);
        }

        private JsonResult Created(JsonResult data, string message)
        {
            return ResponseResult("success", 201, message, data);
        }

        private JsonResult Created(string message)
        {
            return ResponseResult("success", 201, message);
        }

        /// <summary>
        ///     Returns standardised JSON response for a not found request.
        /// </summary>
        /// <remarks>
        ///     ส่งคำตอบ JSON มาตรฐานสำหรับคำขอที่ไม่พบ (ไม่พบข้อมูลดังกล่าว)
        /// </remarks>
        /// <returns>The standardised JSON response.</returns>
        public JsonResult NotFound(string message = "Data not found.")
        {
            return ResponseResult("error", 404, message);
        }

        /// <summary>
        ///     Returns standardised JSON response for a bad request.
        /// </summary>
        /// <remarks>
        ///     ส่งคำตอบ JSON มาตรฐานสำหรับคำขอที่ไม่ถูกต้อง (ผู้ใช้ส่งคำขอไม่ครบถ้วน หรือไม่ถูก Format)
        /// </remarks>
        /// <returns>The standardised JSON response.</returns>
        public JsonResult BadRequest(string message = "Bad request.")
        {
            return ResponseResult("error", 400, message);
        }

        /// <summary>
        ///     Returns standardised JSON response for an internal server error.
        /// </summary>
        /// <remarks>
        ///     ส่งคำตอบ JSON มาตรฐานสำหรับข้อผิดพลาดภายในเซิร์ฟเวอร์ (เซิร์ฟเวอร์มีปัญหา)
        /// </remarks>
        /// <returns>The standardised JSON response.</returns>
        public JsonResult InternalServerError(string message = "Internal server error.")
        {
            return ResponseResult("error", 500, message);
        }

        /// <summary>
        ///     Returns standardised JSON response for an unauthorized request.
        /// </summary>
        /// <remarks>
        ///     ส่งคำตอบ JSON มาตรฐานสำหรับคำขอที่ไม่ได้รับอนุญาต (ไม่มีสิทธิ์ในการเข้าถึงเพราะไม่ได้ล็อกอิน)
        /// </remarks>
        /// <returns>The standardised JSON response.</returns>
        public JsonResult Unauthorized(string message = "Unauthorized.")
        {
            return ResponseResult("error", 401, message);
        }

        /// <summary>
        ///     Returns standardised JSON response for a forbidden request.
        /// </summary>
        /// <remarks>
        ///     ส่งคำตอบ JSON มาตรฐานสำหรับคำขอที่ถูกห้าม (ไม่มีสิทธิ์ในการเข้าถึง ถึงแม้ว่าจะล็อกอินแล้ว)
        /// </remarks>
        /// <returns>The standardised JSON response.</returns>
        public JsonResult Forbidden(string message = "Forbidden.")
        {
            return ResponseResult("error", 403, message);
        }

        /// <summary>
        ///     Returns standardised JSON response for a conflict request.
        /// </summary>
        /// <remarks>
        ///     ส่งคำตอบ JSON มาตรฐานสำหรับคำขอที่ขัดแย้ง (เช่น ข้อมูลซ้ำ)
        /// </remarks>
        /// <returns>The standardised JSON response.</returns>
        public JsonResult Conflict(string message = "Conflict.")
        {
            return ResponseResult("error", 409, message);
        }
    }
}
