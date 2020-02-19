using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Update;

namespace Sparql.Migrator
{
    public class Migrator
    {
        private readonly Options _options;
        private readonly ISparqlQueryProcessor _queryProcessor;
        private readonly ISparqlUpdateProcessor _updateProcessor;

        public Migrator(Options options, ISparqlQueryProcessor queryProcessor, ISparqlUpdateProcessor updateProcessor)
        {
            _options = options;
            _queryProcessor = queryProcessor;
            _updateProcessor = updateProcessor;
            //_updateProcessor = new RemoteUpdateProcessor(_options.Path);
            //_queryProcessor = new RemoteQueryProcessor(new SparqlRemoteEndpoint(new Uri(_options.Path)));

            if (_options.Verbose)
            {
                Console.WriteLine($"Verbose output enabled. Current Arguments: -v {options.Verbose}");
                Console.WriteLine("Quick Start Example! App is in Verbose mode!");
            }
            else
            {
                Console.WriteLine($"Current Arguments: -v {options.Verbose}");
                Console.WriteLine("Quick Start Example!");
            }
        }

        public bool OptionsAreValid(Options o)
        {
            try
            {
                return Directory.Exists(o.Path) && Directory.GetFiles(o.Path, "*.rq").Length > 0 &&
                       Uri.IsWellFormedUriString(o.ServerEndpoint, UriKind.Absolute);
            }
            catch
            {
                return false;
            }
        }

        public void Run()
        {
            var currentState = GetCurrentState();
            StartTransaction();
            try
            {
                foreach (var script in GetScripts())
                {
                    if (ScriptShouldBeRun(script, currentState))
                    {
                        if (RunScript(script, currentState))
                        {
                            WriteUpdateEntry(script, currentState);
                        }
                    }
                }

                CommitTransaction();
            }
            catch (Exception e)
            {
                RollbackTransaction();
                Console.WriteLine(e);
                throw;
            }
        }

        private void WriteUpdateEntry(Script script, CurrentState currentState)
        {
            var mig = CommitNewScriptAsMigrationRecord(script, currentState);
            currentState.AddPreviouslyAppliedMigration(mig);
        }

        private Migration CommitNewScriptAsMigrationRecord(Script script, CurrentState currentState)
        {
            int ord = 0;
            var mostRecentMigration = currentState.Migrations.OrderByDescending(m => m.ordinal).FirstOrDefault();
            if (mostRecentMigration != null)
            {
                ord = ++mostRecentMigration.ordinal;
            }

            var result = new Migration
            {
                migrationHash = script.Hash,
                ordinal = ord,
                originalPath = script.OriginalPath,
                appliedBy = Environment.UserName,
                dtApplied = DateTime.UtcNow,
                migratorVersion = "0.1"
            };
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
            ps.SetLiteral("ordinal", result.ordinal);
            ps.SetLiteral("dtApplied", result.dtApplied);
            ps.SetLiteral("appliedBy", result.appliedBy);
            ps.SetLiteral("migrationHash", result.migrationHash);
            ps.SetLiteral("migratorVersion", result.migratorVersion);
            ps.SetLiteral("originalPath", result.originalPath);
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

            return result;
        }

        private bool RunScript(Script script, CurrentState currentState)
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

        private bool ScriptShouldBeRun(Script script, CurrentState currentState)
        {
            return !currentState.Migrations.Any(m =>
                m.migrationHash.Equals(script.Hash) && m.originalPath.Equals(script.OriginalPath));
        }

        private IEnumerable<Script> GetScripts()
        {
            foreach (var file in Directory.GetFiles(_options.Path, " *.rq"))
            {
                yield return new Script(file);
            }
        }

        private void CommitTransaction()
        {
            // not sure whether there is a nice way to do this over SPARQL 1.1
        }

        private void RollbackTransaction()
        {
            // not sure whether there is a nice way to do this over SPARQL 1.1
        }

        private void StartTransaction()
        {
            // not sure whether there is a nice way to do this over SPARQL 1.1
        }

        private CurrentState GetCurrentState()
        {
            var graph = QueryForCurrentState();
            return ParseGraphIntoCurrentState(graph);
        }

        private CurrentState ParseGraphIntoCurrentState(IGraph g)
        {
            var result = new CurrentState();
            foreach (var mig in g.WalkAll("http://industrialinference.com/migrations/0.1#Migration"))
            {
                var ordinal = mig.DataProperty<int>("mig:ordinal");
                var dtApplied = mig.DataProperty<DateTime>("mig:dtApplied");
                var appliedBy = mig.DataProperty<string>("mig:appliedBy");
                var migrationHash = mig.DataProperty<string>("mig:migrationHash");
                var migratorVersion = mig.DataProperty<string>("mig:migratorVersion");
                var originalPath = mig.DataProperty<string>("mig:originalPath");
                result.AddPreviouslyAppliedMigration(new Migration
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
                CONSTRUCT WHERE {
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
            result?.NamespaceMap.AddNamespace("mig", new Uri("http://industrialinference.com/migrations/0.1#"));
            return result;
        }
    }
}