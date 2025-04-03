using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    ///     Controller สำหรับการดำเนินการคำสั่ง SQL และการส่งผลลัพธ์เป็น JSON
    /// </summary>
    public class LibResponseController : Controller
    {
        private readonly IConfiguration _configuration;

        public LibResponseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        /// <summary>
        ///     ส่งคำตอบ JSON มาตรฐานที่มีสถานะ รหัส และข้อมูลที่ถูกกำหนดไว้
        /// </summary>
        public JsonResult ResponseResult(string status, int code, string message = "", JsonResult data = null)
        {
            if (data == null)
            {
                return new JsonResult(new
                {
                    status = status,
                    code = code,
                    message = message,
                    time = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    data = new { }
                })
                {
                    StatusCode = code
                };
            }
            else
            {
                if (data is JsonResult jsonResult)
                {
                    return new JsonResult(new
                    {
                        status = status,
                        code = code,
                        message = message,
                        time = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        data = data.Value
                    })
                    {
                        StatusCode = code
                    };
                }
                else
                {
                    return new JsonResult(new
                    {
                        status = status,
                        code = code,
                        message = message,
                        time = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        data = data
                    })
                    {
                        StatusCode = code
                    };
                }
            }
        }

        // ฟังก์ชันอื่น ๆ ที่ให้มา
        public JsonResult Success(JsonResult data)
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
