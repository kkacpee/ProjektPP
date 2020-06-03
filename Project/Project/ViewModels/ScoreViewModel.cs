using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.ViewModels
{
    public class ScoreViewModel
    {
        public int ID { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }

        public int Score { get; set; }
        public string Phrase { get; set; }

    }
}
