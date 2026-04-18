using System.ComponentModel.DataAnnotations; // 🔥 ต้องมี

namespace Api.Models
{
    public class AdminRegisterDto
    {
        [Required]
        public int EmpID { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }
    }

    public class AdminLoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}