using System;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Update;

namespace Sparql.Migrator
{
    public class MetadataProvider : IMetadataProvider
    {
        private readonly Options _options;
        private readonly ISparqlQueryProcessor _queryProcessor;
        private readonly ISparqlUpdateProcessor _updateProcessor;

        public MetadataProvider(Options options, ISparqlQueryProcessor queryProcessor, ISparqlUpdateProcessor updateProcessor)
        {
            _options = options;
            _queryProcessor = queryProcessor;
            _updateProcessor = updateProcessor;
        }
        public CurrentState GetCurrentState()
        {
            var graph = QueryForCurrentState();
            return ParseGraphIntoCurrentState(graph);
        }

        private CurrentState ParseGraphIntoCurrentState(IGraph g)
        {
            var result = new CurrentState();
            foreach (var mig in g.WalkAll("mig:Migration"))
            {
                var ordinal = mig.DataProperty<int>("mig:ordinal");
                var dtApplied = mig.DataProperty<DateTime>("mig:dtApplied");
                var appliedBy = mig.DataProperty<string>("mig:appliedBy");
                var migrationHash = mig.DataProperty<string>("mig:migrationHash");
                var migratorVersion = mig.DataProperty<string>("mig:migratorVersion");
                var originalPath = mig.DataProperty<string>("mig:originalPath");
                result.AppendMigration(new Migration
                {
                    ordinal = ordinal,
                    dtApplied = dtApplied,
                    appliedBy = appliedBy,
                    migrationHash = migrationHash,
                    migratorVersion = migratorVersion,
                    originalPath = originalPath
                });
            }

            return result;
        }

        private IGraph QueryForCurrentState()
        {
            // throw new NotImplementedException();
            var ps = new SparqlParameterizedString();
            ps.CommandText = @"
                PREFIX mig: <http://industrialinference.com/migrations/0.1#>
                CONSTRUCT {
		            [] a mig:Migration ;
			            mig:ordinal ?ordinal ;
			            mig:dtApplied ?dtApplied ;
                        mig:appliedBy ?appliedBy ;
                        mig:migrationHash ?migrationHash;
                        mig:migratorVersion ?migratorVersion;
                        mig:originalPath ?originalPath .
                }
                WHERE {
	                GRAPH mig:migrations {
		                _:mig a mig:Migration ;
			                mig:ordinal ?ordinal ;
			                mig:dtApplied ?dtApplied ;
                            mig:appliedBy ?appliedBy ;
                            mig:migrationHash ?migrationHash;
                            mig:migratorVersion ?migratorVersion;
                            mig:originalPath ?originalPath .
                        }
                    }";

            var parser = new SparqlQueryParser();
            var query = parser.ParseFromString(ps);
            var result = _queryProcessor.ProcessQuery(query) as IGraph;
            result?.NamespaceMap.AddNamespace("rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"));
            result?.NamespaceMap.AddNamespace("mig", new Uri("http://industrialinference.com/migrations/0.1#"));
            return result;
        }

        public void RecordSuccessfulMigration(CurrentState currentState, Migration mig)
        {
            var ps = new SparqlParameterizedString();
            ps.CommandText = @"
                PREFIX mig: <http://industrialinference.com/migrations/0.1#>
                INSERT DATA {
	                GRAPH mig:migrations {
		                _:mig a mig:Migration ;
			                mig:ordinal @ordinal ;
			                mig:dtApplied @dtApplied ;
                            mig:appliedBy @appliedBy ;
                            mig:migrationHash @migrationHash;
                            mig:migratorVersion @migratorVersion;
                            mig:originalPath @originalPath .
                        }
                    }";
            ps.SetLiteral("ordinal", mig.ordinal);
            ps.SetLiteral("dtApplied", mig.dtApplied);
            ps.SetLiteral("appliedBy", mig.appliedBy);
            ps.SetLiteral("migrationHash", mig.migrationHash);
            ps.SetLiteral("migratorVersion", mig.migratorVersion);
            ps.SetLiteral("originalPath", mig.originalPath);
            var parser = new SparqlUpdateParser();
            var query = parser.ParseFromString(ps);
            try
            {
                query.Process(_updateProcessor);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public bool OptionsAreValid(Options o)
        {
            return Uri.IsWellFormedUriString(o.ServerEndpoint, UriKind.Absolute);
        }
    }
}