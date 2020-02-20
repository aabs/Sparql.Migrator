namespace Sparql.Migrator
{
    public interface IScriptApplicator : IOptionsValidator
    {
        bool ApplyScript(Script script);
    }
}