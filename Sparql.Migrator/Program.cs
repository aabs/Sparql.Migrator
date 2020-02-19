using System;
using System.IO;
using System.IO.Abstractions;
using CommandLine;
using CommandLine.Text;
using VDS.RDF.Query;
using VDS.RDF.Update;

namespace Sparql.Migrator
{
    class Program
    {

        static void Main(string[] args)
        {
            var parserResult = Parser.Default.ParseArguments<Options>(args);
            parserResult
                .WithParsed<Options>(o =>
                {
                    var updateProcessor = new RemoteUpdateProcessor(o.Path);
                    var queryProcessor = new RemoteQueryProcessor(new SparqlRemoteEndpoint(new Uri(o.Path)));

                    var migrator = new Migrator(o, queryProcessor, updateProcessor, new FileSystem());
                    if (!migrator.OptionsAreValid(o))
                    {
                        Console.WriteLine("Sorry the options are not valid");
                        Console.WriteLine(HelpText.RenderUsageText(parserResult));
                    }
                    migrator.Run();
                });
        }
    }
}
