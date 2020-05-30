﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public class SQLScoreRepository : IScoreRepository
    {

        private readonly AppDbContext context;
        private readonly ILogger<SQLScoreRepository> logger;

        public SQLScoreRepository(AppDbContext context,
                                   ILogger<SQLScoreRepository> logger)
        {
            this.logger = logger;
            this.context = context;
        }
        public Score Add(Score score)
        {
            context.Scores.Add(score);
            context.SaveChanges();
            return score;
        }

        public Score Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Score> GetAllScores()
        {
            return context.Scores;
        }

        public Score GetScore(int Id)
        {
            throw new NotImplementedException();
        }

        public Score Update(Score scoreChanges)
        {
            throw new NotImplementedException();
        }
    }
}
