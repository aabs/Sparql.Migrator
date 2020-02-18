using System;
using System.IO;

namespace Sparql.Migrator
{
    public class Migrator
    {
        private readonly Options _options;

        public Migrator(Options options)
        {
            if (OptionsAreValid(options))
            {
                _options = options;

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
        }

        private bool OptionsAreValid(Options o)
        {
            return Directory.Exists(o.Path) && Directory.GetFiles(o.Path, "*.rq").Length > 0 && Uri.IsWellFormedUriString(o.ServerEndpoint, UriKind.Absolute);
        }

        public void Run()
        {
            throw new NotImplementedException();
        }
    }
}