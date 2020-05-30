using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public interface IScoreRepository
    {
        Score GetScore(int Id);
        IEnumerable<Score> GetAllScores();
        Score Add(Score score);
        Score Update(Score scoreChanges);
        Score Delete(int id);
    }
}
