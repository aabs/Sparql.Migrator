using System;
using System.Collections.Generic;
using System.IO;
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
    public class MetadataProviderTests
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
            trigparser.Load(ts,
                Path.Combine("..", "..", "..", "..", "test-data", "sample-metadata",
                    "20200218T1100-tracking-metadata.trig"));
            var g = ts.Graphs[new Uri("http://industrialinference.com/migrations/0.1#migrations")];
            return g;
        }

        [Test]
        public void TestCanCreate()
        {
            var sut = new MetadataProvider(Mock.Of<Options>(), Mock.Of<ISparqlQueryProcessor>(), Mock.Of<ISparqlUpdateProcessor>());
            sut.ShouldNotBeNull();
        }

        [Test]
        public void TestCanParseTheTestData()
        {
            var ts = new TripleStore();
            var options = Mock.Of<Options>();
            var queryProcessor = new Mock<ISparqlQueryProcessor>();
            queryProcessor.Setup(p => p.ProcessQuery(It.IsAny<SparqlQuery>())).Returns(LoadTestData(ts));
            var updateProcessor = Mock.Of<ISparqlUpdateProcessor>();
            var sut = new MetadataProvider(options, queryProcessor.Object, updateProcessor);
            var actual = sut.GetCurrentState();
            actual.ShouldNotBeNull();
            actual.Migrations.ShouldNotBeEmpty();
            actual.Migrations.ElementAt(0).ordinal.ShouldBe(1);
            actual.Migrations.ElementAt(1).ordinal.ShouldBe(2);
            actual.Migrations.ElementAt(0).dtApplied.ShouldBe(new DateTime(2020,02,19,13,45,22));
            actual.Migrations.ElementAt(1).dtApplied.ShouldBe(new DateTime(2020,02,19,13,45,22), TimeSpan.FromMilliseconds(125)); // no precision beyond milliseconds?
            actual.Migrations.ElementAt(0).migrationHash.ShouldBe("abc123");
            actual.Migrations.ElementAt(1).migrationHash.ShouldBe("def234");
            actual.Migrations.ElementAt(0).originalPath.ShouldBe("./test-data/migrations/20200218T1200-drop.rq");
            actual.Migrations.ElementAt(1).originalPath.ShouldBe("./test-data/migrations/20200218T1300-test1.rq");
        }
    }
}