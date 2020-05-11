using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public interface IEngineRepository
    {
        Engine GetEngine(int Id);
        IEnumerable<Engine> GetAllEngines();
        Engine Add(Engine engine);
        Engine Update(Engine engineChanges);
        Engine Delete(int id);
    }
}
