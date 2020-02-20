using System;
using System.Linq;

namespace Sparql.Migrator
{
    public class Migrator : IMigrator
    {
        private readonly IMetadataProvider _metadataProvider;
        private readonly IScriptApplicator _scriptApplicator;
        private readonly IScriptProvider _scriptProvider;
        private readonly ITransactionProvider _transactionProvider;

        public Migrator(IScriptProvider scriptProvider, IScriptApplicator scriptApplicator,
            IMetadataProvider metadataProvider, ITransactionProvider transactionProvider)
        {
            _scriptProvider = scriptProvider;
            _scriptApplicator = scriptApplicator;
            _metadataProvider = metadataProvider;
            _transactionProvider = transactionProvider;
        }

        public bool OptionsAreValid(Options o)
        {
            return _scriptProvider.OptionsAreValid(o) && _scriptApplicator.OptionsAreValid(o) &&
                   _metadataProvider.OptionsAreValid(o);
        }

        public void Run()
        {
            var currentState = _metadataProvider.GetCurrentState();
            _transactionProvider.Start();
            try
            {
                foreach (var script in _scriptProvider.GetAllScripts())
                {
                    if (ScriptShouldBeRun(script, currentState))
                    {
                        if (_scriptApplicator.ApplyScript(script))
                        {
                            WriteUpdateEntry(script, currentState);
                        }
                    }
                }

                _transactionProvider.Commit();
            }
            catch (Exception e)
            {
                _transactionProvider.Rollback();
                Console.WriteLine(e);
                throw;
            }
        }

        public void WriteUpdateEntry(Script script, CurrentState currentState)
        {
            var mig = CommitNewScriptAsMigrationRecord(script, currentState);
            currentState.AddPreviouslyAppliedMigration(mig);
        }

        public Migration CommitNewScriptAsMigrationRecord(Script script, CurrentState currentState)
        {
            int ord = 0;
            var mostRecentMigration = currentState.Migrations.OrderByDescending(m => m.ordinal).FirstOrDefault();
            if (mostRecentMigration != null)
            {
                ord = ++mostRecentMigration.ordinal;
            }

            var mig = new Migration
            {
                migrationHash = script.Hash,
                ordinal = ord,
                originalPath = script.OriginalPath,
                appliedBy = Environment.UserName,
                dtApplied = DateTime.UtcNow,
                migratorVersion = "0.1"
            };

            try
            {
                _metadataProvider.OnNewScriptApplication(currentState, mig);
            }
            catch
            {
                Console.WriteLine("Unable to update metadata");
                throw;
            }

            return mig;
        }

        public bool ScriptShouldBeRun(Script script, CurrentState currentState)
        {
            var scriptShouldBeRun = !currentState.Migrations.Any(delegate(Migration m)
            {
                var hashesEqual = m.migrationHash.Equals(script.Hash);
                var pathsEqual = m.originalPath.Equals(script.OriginalPath);
                return hashesEqual && pathsEqual;
            });
            return scriptShouldBeRun;
        }
    }
}