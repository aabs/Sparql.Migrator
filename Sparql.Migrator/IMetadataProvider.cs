namespace Sparql.Migrator
{
    public interface IMetadataProvider : IOptionsValidator
    {
        CurrentState GetCurrentState();
        void OnNewScriptApplication(CurrentState state, Migration mig);
    }
}