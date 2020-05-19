using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.ViewModels
{
    public class HistoriesWithNames : History
    {
        public HistoriesWithNames( History history, string userName, string phrase)
        {
            ID = history.ID;
            SearchId = history.SearchId;
            UserId = history.UserId;
            Date = history.Date;
            Phrase = phrase;
            UserName = userName;
        }

        public string Phrase { get; set; }
        public string UserName { get; set; }

    }
}
