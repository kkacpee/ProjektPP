using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public class Result
    {
        public int ID { get; set; }
        [ForeignKey("Search")]
        public int SearchId { get; set; }
        [ForeignKey("Photo")]
        public int PhotoId { get; set; }
        [ForeignKey("Engine")]
        public int EngineId { get; set; }
        public int AvgScore { get; set; }

        public virtual Search Search { get; set; }
        public virtual Photo Photo { get; set; }
        public virtual Engine Engine { get; set; }
    }
}
