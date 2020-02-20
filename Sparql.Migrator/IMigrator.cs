namespace Sparql.Migrator
{
    public interface IMigrator
    {
        bool OptionsAreValid(Options o);
        void Run();
    }
}