namespace Api.Models
{
    public class NotificationRequest
    {
        public int EmpID { get; set; }          // รหัสพนักงาน
        public string? ToProcess { get; set; }  // กระบวนการที่ถูกย้ายไป
        public string? ToBiz { get; set; }      // หน่วยธุรกิจปลายทาง
        public string? Title { get; set; }      // ชื่อหัวข้อการแจ้งเตือน
        public string? Message { get; set; }    // เนื้อหาการแจ้งเตือน
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
