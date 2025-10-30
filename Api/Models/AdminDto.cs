namespace Api.Models
{
    public class AdminRegisterDto
    {
        public int EmpID { get; set; }      // เพิ่มฟิลด์ EmpID
        public string FirstName { get; set; }  // เพิ่มฟิลด์ FirstName
        public string LastName { get; set; }   // เพิ่มฟิลด์ LastName
        public string Username { get; set; }  // ฟิลด์ Username
        public string Password { get; set; }  // รหัสผ่านธรรมดาจาก frontend
        public string Email { get; set; }     // ฟิลด์อีเมล
    }

    public class AdminLoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
