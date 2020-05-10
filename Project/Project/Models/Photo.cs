using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public class Photo
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public string Url { get; set; }
        public int ResX { get; set; }
        public int ResY { get; set; }
        public string PhotoFormat { get; set; }
        [Required]
        public DateTime Date { get; set; }
    }
}
