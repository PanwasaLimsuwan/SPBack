using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    ///     Controller สำหรับการดำเนินการคำสั่ง SQL และการส่งผลลัพธ์เป็น JSON
    /// </summary>
    public class LibResponseController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LibResponseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        ///     ส่งคำตอบ JSON มาตรฐานที่มีสถานะ รหัส และข้อมูลที่ถูกกำหนดไว้
        /// </summary>
        public JsonResult ResponseResult(string status, int code, string message = "", object data = null)
        {
            return new JsonResult(new
            {
                status = status,
                code = code,
                message = message,
                time = DateTimeOffset.Now.ToUnixTimeSeconds(),
                data = data ?? new { }
            })
            {
                StatusCode = code
            };
        }

        public JsonResult Success(object data)
        {
            return ResponseResult("success", 200, "Data retrieved successfully.", data);
        }

        public JsonResult NotFound(string message = "Data not found.")
        {
            return ResponseResult("error", 404, message);
        }

        public JsonResult BadRequest(string message = "Bad request.")
        {
            return ResponseResult("error", 400, message);
        }

        public JsonResult InternalServerError(string message = "Internal server error.")
        {
            return ResponseResult("error", 500, message);
        }
    }
}
