using System;
using System.IO;
using System.IO.Abstractions;
using Autofac;
using CommandLine;
using CommandLine.Text;
using VDS.RDF.Query;
using VDS.RDF.Update;

namespace Sparql.Migrator
{
    class Program
    {
        private static IContainer _container;

        static void Main(string[] args)
        {
            var parserResult = Parser.Default.ParseArguments<Options>(args);
            parserResult
                .WithParsed<Options>(o =>
                {
                    SetupIocContainer(o);
                    using (var scope = _container.BeginLifetimeScope())
                    {
                        var migrator = scope.Resolve<IMigrator>();
                        migrator.Run();
                    }
                });
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
            builder.RegisterType<MetadataProvider>().AsImplementedInterfaces();
            builder.RegisterType<ScriptApplicator>().AsImplementedInterfaces();
            builder.RegisterType<ScriptProvider>().AsImplementedInterfaces();
            builder.RegisterType<TransactionProvider>().AsImplementedInterfaces();
            _container = builder.Build();
        }
    }
}
