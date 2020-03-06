using System;
using System.Collections.Generic;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Update;

namespace Sparql.Migrator
{
    public class Purger : IPurger
    {
        private readonly ISparqlUpdateProcessor _updateProcessor;

        public Purger(ISparqlUpdateProcessor updateProcessor)
        {
            _updateProcessor = updateProcessor;
        }

        public void Run()
        {
            var ps = new SparqlParameterizedString();
            ps.CommandText = @"DROP SILENT GRAPH <http://industrialinference.com/migrations/0.1#migrations>; DROP DEFAULT;";
            var parser = new SparqlUpdateParser();
            var query = parser.ParseFromString(ps);
            try
            {
                query.Process(_updateProcessor);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public interface IPurger
    {
        public void Run();
    }
}
