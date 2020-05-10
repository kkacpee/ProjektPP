using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public class Search
    {
        public int ID { get; set; }
        [Required]
        public string Phrase { get; set; }
        [Required]
        public DateTime Date { get; set; }
    }
}
