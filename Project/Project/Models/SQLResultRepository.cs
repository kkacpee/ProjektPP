using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public class SQLResultRepository : IResultRepository
    {
        private readonly AppDbContext context;
        private readonly ILogger<SQLResultRepository> logger;

        public SQLResultRepository(AppDbContext context,
                                   ILogger<SQLResultRepository> logger)
        {
            this.logger = logger;
            this.context = context;
        }

        public Result Add(Result result)
        {
            context.Results.Add(result);
            context.SaveChanges();
            return result;
        }

        public Result Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Result> GetAllResults()
        {
            return context.Results;
        }

        public Result GetResult(int Id)
        {
            return context.Results.Find(Id);
        }

        public IEnumerable<Result> GetResultsForSearch(int id)
        {
            return context.Results.Where(d => d.SearchId == id);
        }

        public Result Update(Result resultChanges)
        {
            var result = context.Results.Attach(resultChanges);
            result.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
            return resultChanges;
        }
    }
}
