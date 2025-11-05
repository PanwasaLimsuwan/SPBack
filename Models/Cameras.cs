using System;
using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Cameras
    {
        [Key]
        public string CameraID { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
    }
}
