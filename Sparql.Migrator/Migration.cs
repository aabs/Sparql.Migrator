using System;

namespace Sparql.Migrator
{
    public class Migration
    {
        public string appliedBy;
        public DateTime dtApplied;
        public string migrationHash;
        public string migratorVersion;
        public int ordinal;
        public string originalPath;
    }
}