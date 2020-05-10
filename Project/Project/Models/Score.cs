using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public class Score
    {
        public int ID { get; set; }
        [Required]
        public int Grade { get; set; }
        [ForeignKey("IdentityUser")]
        public string UserId { get; set; }
        [ForeignKey("Result")]
        public int ResultId { get; set; }

        public virtual IdentityUser IdentityUser { get; set; }
        public virtual Result Result { get; set; }


    }
}
