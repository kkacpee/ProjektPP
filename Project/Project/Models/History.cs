using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public class History
    {
        [Key]
        public int ID { get; set; }
        public DateTime Date { get; set; }
        [ForeignKey("IdentityUser")]
        public string UserId { get; set; }
        [ForeignKey("Search")]
        public int SearchId { get; set; }

        public virtual Search Search { get; set; }
        public virtual IdentityUser IdentityUser { get; set; }
    }
}
