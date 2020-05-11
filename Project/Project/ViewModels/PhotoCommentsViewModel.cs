using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.ViewModels
{
    public class PhotoCommentsViewModel
    {
        public int ID { get; set; }
        public string Url { get; set; }
        public int ResX { get; set; }
        public int ResY { get; set; }
        public string PhotoFormat { get; set; }
        public DateTime Date { get; set; }
        public IEnumerable<CommentWithName> Comments { get; set; }
    }
}
