using CommandLine;

namespace Sparql.Migrator
{
    public class Options
    {
        [Option('v', "verbose", Required = false, Default = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('s', "server", Required = true, HelpText = "Full URI of read/write endpoint of Triple Store.")]
        public string ServerEndpoint { get; set; }

        [Option('p', "path", Required = true, Default = "./Migrations", HelpText = "Root path of migration query scripts.")]
        public string Path { get; set; }

    }
}