using System.Collections.Generic;

namespace Sparql.Migrator
{
    internal class CurrentState
    {
        public List<Migration> Migrations => new List<Migration>();
        public void AddPreviouslyAppliedMigration(Migration mig)
        {
            Migrations.Add(mig);
        }
    }
}