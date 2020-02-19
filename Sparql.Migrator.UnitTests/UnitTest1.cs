using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using AutoFixture;
using Moq;
using NUnit.Framework;
using Shouldly;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Update;

namespace Sparql.Migrator.UnitTests
{
    public class Tests
    {
        private IFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
        }

        private IGraph LoadTestData(TripleStore ts)
        {
            TriGParser trigparser = new TriGParser();
            trigparser.Load(ts, Path.Combine("..", "..", "..", "..", "test-data", "sample-metadata", "20200218T1100-tracking-metadata.trig"));
            var g = ts.Graphs[new Uri("http://industrialinference.com/migrations/0.1#migrations")];
            return g;
        }

        [Test]
        public void Test1()
        {
            var ts = new TripleStore();
            var opts = _fixture.Create<Options>();
            opts.Path = Path.Combine("..", "..", "..", "..", "test-data", "migrations");
            opts.ServerEndpoint = "http://localhost:8889/blazegraph/namespace/kb/sparql";
            var qp = new Mock<ISparqlQueryProcessor>();
            qp.Setup(p => p.ProcessQuery(It.IsAny<SparqlQuery>())).Returns(LoadTestData(ts));
            var up = Mock.Of<ISparqlUpdateProcessor>();
            // var fs = Mock.Of<IFileSystem>();
            var sut = new Migrator(opts, qp.Object, up, new FileSystem());
            sut.Run();
        }

        [Test]
        public void Test2()
        {
            var ts = new TripleStore();
            LoadTestData(ts);
            var opts = _fixture.Create<Options>();
            opts.Path = Path.Combine("..", "..", "..", "..", "test-data", "migrations");
            opts.ServerEndpoint = "http://localhost:8889/blazegraph/namespace/kb/sparql";
            var qp = new LeviathanQueryProcessor(ts);
            var up = new LeviathanUpdateProcessor(ts);
            var sut = new Migrator(opts, qp, up, new FileSystem());
            ts.Triples.Count().ShouldBe(14);
            sut.Run();
            ts.Triples.Count().ShouldBe(28);
        }
    }
}