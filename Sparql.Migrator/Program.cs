using System;
using System.IO;
using CommandLine;
using CommandLine.Text;

namespace Sparql.Migrator
{
    class Program
    {

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    var migrator = new Migrator(o);
                    migrator.Run();
                });
        }
    }
}
