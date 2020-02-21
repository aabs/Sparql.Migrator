using System.Collections.Generic;

namespace Sparql.Migrator
{
    public class CurrentState
    {
        private readonly List<Migration> _migrations = new List<Migration>();
        public void AppendMigration(Migration mig)
        {
            _migrations.Add(mig);
        }

        public List<Migration> Migrations
        {
            get
            {
                return _migrations;
            }
        }
    }
}