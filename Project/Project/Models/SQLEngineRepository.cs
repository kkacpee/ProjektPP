using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public class SQLEngineRepository : IEngineRepository
    {
        private readonly AppDbContext context;
        private readonly ILogger<SQLEngineRepository> logger;

        public SQLEngineRepository(AppDbContext context,
                                    ILogger<SQLEngineRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }
        public Engine Add(Engine engine)
        {
            context.Engines.Add(engine);
            context.SaveChanges();
            return engine;
        }

        public Engine Delete(int id)
        {
            Engine engine = context.Engines.Find(id);
            if (engine != null)
            {
                context.Engines.Remove(engine);
                context.SaveChanges();
            }
            return engine;
        }

        public IEnumerable<Engine> GetAllEngines()
        {
            return context.Engines;
        }

        public Engine GetEngine(int Id)
        {
            return context.Engines.Find(Id);
        }

        public Engine Update(Engine engineChanges)
        {
            var engine = context.Engines.Attach(engineChanges);
            engine.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
            return engineChanges;
        }
    }
}
