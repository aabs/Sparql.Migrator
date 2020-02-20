namespace Sparql.Migrator
{
    public interface ITransactionProvider
    {
        void Start();
        void Commit();
        void Rollback();
    }

    class TransactionProvider : ITransactionProvider
    {
        public void Start()
        {
        }

        public void Commit()
        {
        }

        public void Rollback()
        {
        }
    }
}