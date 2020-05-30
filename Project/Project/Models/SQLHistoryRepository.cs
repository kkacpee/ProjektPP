using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public class SQLHistoryRepository : IHistoryRepository
    {

        private readonly AppDbContext context;
        private readonly ILogger<SQLHistoryRepository> logger;

        public SQLHistoryRepository(AppDbContext context,
                                   ILogger<SQLHistoryRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public History Add(History history)
        {
            context.Histories.Add(history);
            context.SaveChanges();
            return history;
        }

        public History Delete(int id)
        {
            History history = context.Histories.Find(id);
            if (history != null)
            {
                context.Histories.Remove(history);
                context.SaveChanges();
            }
            return history;
        }

        public IEnumerable<History> GetAllHistories()
        {
            return context.Histories;
        }

        public History GetHistory(int Id)
        {
            return context.Histories.Where(d => d.SearchId == Id).FirstOrDefault();
        }

        public IEnumerable<History> GetHistoryForUser(string id)
        {
            return context.Histories.Where(d => d.UserId == id);
        }

        public History Update(History historyChanges)
        {
            throw new NotImplementedException();
        }
    }
}
