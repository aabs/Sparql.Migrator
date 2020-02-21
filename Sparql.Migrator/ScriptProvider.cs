using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Sparql.Migrator
{
    public class ScriptProvider : IScriptProvider
    {
        private readonly Options _options;
        private readonly IFileSystem _fileSystem;

        public ScriptProvider(Options options, IFileSystem fileSystem)
        {
            _options = options;
            _fileSystem = fileSystem;
        }
        public IEnumerable<Script> GetAllScripts()
        {
            using SHA256 sha256Hash = SHA256.Create();
            var result = new LinkedList<Script>();
            foreach (var s in _fileSystem.Directory.GetFiles(_options.Path, "*.rq"))
            {
                var contents = _fileSystem.File.ReadAllText(s);
                result.AddLast(new Script(contents, s, DateTime.UtcNow, GetHash(sha256Hash, contents))) ;
            }

            return result.OrderBy(s => Path.GetFileNameWithoutExtension(s.OriginalPath));
        }

        public bool OptionsAreValid(Options o)
        {
            try
            {
                return _fileSystem.Directory.Exists(o.Path) && _fileSystem.Directory.GetFiles(o.Path, "*.rq").Length > 0 ;
            }
            catch
            {
                return false;
            }
        }
        private static string GetHash(HashAlgorithm hashAlgorithm, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

    }
}