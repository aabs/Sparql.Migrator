namespace Sparql.Migrator
{
    public interface IMetadataProvider : IOptionsValidator
    {
        CurrentState GetCurrentState();
        void RecordSuccessfulMigration(CurrentState state, Migration mig);
    }
}