using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public interface ISearchRepository
    {
        Search Add(Search search);
        IEnumerable<Search> GetAllSearches();
    }
}
