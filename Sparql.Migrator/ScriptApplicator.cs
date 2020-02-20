using System;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Update;

namespace Sparql.Migrator
{
    public class ScriptApplicator : IScriptApplicator
    {
        private readonly Options _options;
        private readonly ISparqlQueryProcessor _queryProcessor;
        private readonly ISparqlUpdateProcessor _updateProcessor;

        public ScriptApplicator(Options options, ISparqlQueryProcessor queryProcessor, ISparqlUpdateProcessor updateProcessor)
        {
            _options = options;
            _queryProcessor = queryProcessor;
            _updateProcessor = updateProcessor;
        }
        public bool ApplyScript(Script script)
        {
            var ps = new SparqlParameterizedString();
            ps.CommandText = script.Contents;
            var parser = new SparqlUpdateParser();
            var query = parser.ParseFromString(ps);
            try
            {
                query.Process(_updateProcessor);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public bool OptionsAreValid(Options o)
        {
            return Uri.IsWellFormedUriString(o.ServerEndpoint, UriKind.Absolute);
        }
    }
}