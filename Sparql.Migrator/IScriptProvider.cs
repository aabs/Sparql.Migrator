using System.Collections.Generic;

namespace Sparql.Migrator
{
    public interface IScriptProvider : IOptionsValidator
    {
        IEnumerable<Script> GetAllScripts();
    }
}