using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Autofac;
using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;
using VDS.RDF.Query;
using VDS.RDF.Update;

namespace Sparql.Migrator
{
    class Program : ConsoleAppBase
    {
        private static IContainer _container;

        static async Task Main(string[] args)
        {
            // target T as ConsoleAppBase.
            await Host.CreateDefaultBuilder().RunConsoleAppFrameworkAsync<Program>(args);
        }

        [Command("purge")]
        public void Purge(
            [Option("s", "Full URI of read/write endpoint of Triple Store.")]
            string server)
        {
            var options = new Options {ServerEndpoint = server, Path = "", Verbose = true};
            SetupIocContainer(options);
            using (var scope = _container.BeginLifetimeScope())
            {
                var purger = scope.Resolve<IPurger>();
                purger.Run();
            }
        }

        [Command("migrate")]
        public void Migrate(
            [Option("s", "Full URI of read/write endpoint of Triple Store.")]
            string server,
            [Option("p", "Root path of migration query scripts.")]
            string scripts,
            [Option("v", "Set output to verbose messages.")]
            bool verbose = false)
        {
            var options = new Options {ServerEndpoint = server, Path = scripts, Verbose = verbose};
            SetupIocContainer(options);
            using (var scope = _container.BeginLifetimeScope())
            {
                var migrator = scope.Resolve<IMigrator>();
                migrator.Run();
            }
        }

        private static void SetupIocContainer(Options o)
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(o).As<Options>();
            builder.RegisterInstance(new RemoteQueryProcessor(new SparqlRemoteEndpoint(new Uri(o.ServerEndpoint))))
                .As<ISparqlQueryProcessor>();
            builder.RegisterInstance(new RemoteUpdateProcessor(o.ServerEndpoint))
                .As<ISparqlUpdateProcessor>();
            builder.RegisterType<FileSystem>().As<IFileSystem>();
            builder.RegisterType<Migrator>().AsImplementedInterfaces();
            builder.RegisterType<Purger>().AsImplementedInterfaces();
            builder.RegisterType<MetadataProvider>().AsImplementedInterfaces();
            builder.RegisterType<ScriptApplicator>().AsImplementedInterfaces();
            builder.RegisterType<ScriptProvider>().AsImplementedInterfaces();
            builder.RegisterType<TransactionProvider>().AsImplementedInterfaces();
            _container = builder.Build();
        }
    }
}