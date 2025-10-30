using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Widget
    {
        [Key] // Primary Key
        public int id { get; set; }

        [Required]
        public int user_id { get; set; }

        [Required]
        public string settings { get; set; }
    }
}

