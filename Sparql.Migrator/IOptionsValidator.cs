namespace Sparql.Migrator
{
    public interface IOptionsValidator
    {
        bool OptionsAreValid(Options o);
    }
}