using System;

namespace Sparql.Migrator
{
    public class Script
    {
        public Script(string contents, string originalPath, DateTime retrievedAt, string hash)
        {
            RetrievedAt = retrievedAt;
            Contents = contents;
            Hash = hash;
            OriginalPath = originalPath;
        }

        public DateTime RetrievedAt { get; set; }
        public string Contents { get; set; }
        public string Hash { get; set; }
        public string OriginalPath { get; set; }
    }
}