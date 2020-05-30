using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public interface IResultRepository
    {
        Result GetResult(int Id);
        IEnumerable<Result> GetAllResults();
        Result Add(Result result);
        Result Update(Result resultChanges);
        Result Delete(int id);

        IEnumerable<Result> GetResultsForSearch(int id);
    }
}
