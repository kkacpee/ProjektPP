using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public interface IHistoryRepository
    {
        History GetHistory(int Id);
        IEnumerable<History> GetAllHistories();

        IEnumerable<History> GetHistoryForUser(string id);
        History Add(History history);
        History Update(History historyChanges);
        History Delete(int id);
    }
}
