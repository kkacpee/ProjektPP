using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public class SQLSearchRepository : ISearchRepository
    {
        private readonly AppDbContext context;
        private readonly ILogger<SQLPhotoRepository> logger;

        public SQLSearchRepository(AppDbContext context,
                                   ILogger<SQLPhotoRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public Search Add(Search search)
        {
            context.Searches.Add(search);
            context.SaveChanges();
            return search;
        }
    }
}
