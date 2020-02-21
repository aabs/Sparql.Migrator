using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoFixture;
using Moq;
using NUnit.Framework;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Sparql.Migrator.UnitTests
{
    public class MigratorTests
    {
        private IFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
        }


        [Test]
        public void TestCallsGetCurrentStateOnce()
        {
            var scriptProvider = Mock.Of<IScriptProvider>();
            var scriptApplicator = Mock.Of<IScriptApplicator>();
            var transactionProvider = Mock.Of<ITransactionProvider>();
            var metadataProvider = new Mock<IMetadataProvider>();
            metadataProvider.Setup(mp => mp.GetCurrentState()).Returns(CreateMockState(3));

            var sut = new Migrator(scriptProvider, scriptApplicator, metadataProvider.Object, transactionProvider);
            sut.Run();
            metadataProvider.Verify(mp => mp.GetCurrentState(), Times.Once);
        }

        [TestCase(0, 0)]
        [TestCase(1, 0)]
        [TestCase(2, 0)]
        [TestCase(0, 1)]
        [TestCase(1, 1)]
        [TestCase(2, 1)]
        [TestCase(0, 2)]
        [TestCase(1, 2)]
        [TestCase(2, 2)]
        public void TestScriptApplication_IsRunOnlyForUnknownScripts(int quantityOfPriorMigrations, int extraScripts)
        {
            var currentState = CreateMockState(quantityOfPriorMigrations);
            var scriptProvider = new Mock<IScriptProvider>();
            scriptProvider.Setup(sp => sp.GetAllScripts())
                .Returns(CreateMockScripts(quantityOfPriorMigrations + extraScripts));
            var scriptApplicator = new Mock<IScriptApplicator>();
            scriptApplicator.Setup(sa => sa.ApplyScript(It.IsAny<Script>())).Returns(true);
            var transactionProvider = new Mock<ITransactionProvider>();
            var metadataProvider = new Mock<IMetadataProvider>();
            metadataProvider.Setup(mp => mp.GetCurrentState()).Returns(currentState);

            var sut = new Migrator(scriptProvider.Object, scriptApplicator.Object, metadataProvider.Object,
                transactionProvider.Object);

            sut.Run();
            scriptApplicator.Verify(sa => sa.ApplyScript(It.IsAny<Script>()), Times.Exactly(extraScripts));
            metadataProvider.Verify(
                mp => mp.RecordSuccessfulMigration(
                    It.Is<CurrentState>(c => c.Equals(currentState)),
                    It.IsAny<Migration>()), Times.Exactly(extraScripts));
        }

        [TestCase(0, 0, true)]
        [TestCase(1, 0, true)]
        [TestCase(2, 0, true)]
        [TestCase(0, 1, true)]
        [TestCase(1, 1, true)]
        [TestCase(2, 1, true)]
        [TestCase(0, 2, true)]
        [TestCase(1, 2, true)]
        [TestCase(2, 2, true)]

        [TestCase(0, 0, false)]
        [TestCase(1, 0, false)]
        [TestCase(2, 0, false)]
        [TestCase(0, 1, false)]
        [TestCase(1, 1, false)]
        [TestCase(2, 1, false)]
        [TestCase(0, 2, false)]
        [TestCase(1, 2, false)]
        [TestCase(2, 2, false)]
        public void TestStateUpdate_IsOnlyRunIfApplicationIsSuccessful(int quantityOfPriorMigrations, int extraScripts, bool applicationIsSuccessful)
        {
            var currentState = CreateMockState(quantityOfPriorMigrations);
            var scriptProvider = new Mock<IScriptProvider>();
            scriptProvider.Setup(sp => sp.GetAllScripts())
                .Returns(CreateMockScripts(quantityOfPriorMigrations + extraScripts));
            var scriptApplicator = new Mock<IScriptApplicator>();
            scriptApplicator.Setup(sa => sa.ApplyScript(It.IsAny<Script>())).Returns(applicationIsSuccessful);
            var transactionProvider = new Mock<ITransactionProvider>();
            var metadataProvider = new Mock<IMetadataProvider>();
            metadataProvider.Setup(mp => mp.GetCurrentState()).Returns(currentState);

            var sut = new Migrator(scriptProvider.Object, scriptApplicator.Object, metadataProvider.Object,
                transactionProvider.Object);

            sut.Run();
            scriptApplicator.Verify(sa => sa.ApplyScript(It.IsAny<Script>()), Times.Exactly(extraScripts));
            metadataProvider.Verify(
                mp => mp.RecordSuccessfulMigration(
                    It.Is<CurrentState>(c => c.Equals(currentState)),
                    It.IsAny<Migration>()), Times.Exactly(applicationIsSuccessful?extraScripts:0));
        }

        private CurrentState CreateMockState(int quantityOfPriorMigrations)
        {
            var result = new CurrentState();
            foreach (var i in Enumerable.Range(0, quantityOfPriorMigrations))
            {
                result.AppendMigration(new Migration
                {
                    ordinal = i,
                    appliedBy = "andrewm",
                    dtApplied = DateTime.Today.AddDays(-1 * i),
                    migrationHash = $"abc123{i}",
                    migratorVersion = "0.1",
                    originalPath = $"/home/blah/path/file{i}.rq"
                });
            }

            return result;
        }

        private IEnumerable<Script> CreateMockScripts(int quantityOfScripts)
        {
            for (int i = 0; i < quantityOfScripts; i++)
            {
                yield return new Script($"contents {i}", $"/home/blah/path/file{i}.rq", DateTime.Today.AddDays(-1 * i),
                    $"abc123{i}");
            }
        }
    }
}