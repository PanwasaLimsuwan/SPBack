using System;

namespace Api.Models
{
    public class AdminRegisterDto
    {
        public string Username { get; set; }
        public string Password { get; set; } // รหัสผ่านธรรมดาจาก frontend
    }

    public class AdminLoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
