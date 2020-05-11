using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public class Comment
    {
        public int ID { get; set; }

        [ForeignKey("Photo")]
        public int PhotoId { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime Date { get; set; }
        [ForeignKey("IdentityUser")]
        public string UserId { get; set; }
        public virtual IdentityUser IdentityUser { get; set; }
        public virtual Photo Photo { get; set; }

    }
}
