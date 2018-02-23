using NetCoreBootstrap.Persistance.Interfaces;
using src.Persistance.Repositories;

namespace NetCoreBootstrap.Persistance
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataBaseContext _context;
        public UnitOfWork(DataBaseContext context)
        {
            this._context = context;
            this.ExampleModelRepository = new ExampleModelRepository(_context);// TODO DELETE THIS
        }

        public IExampleModelRepository ExampleModelRepository { get; private set; }// TODO DELETE THIS


        public int Complete()
        {
            return this._context.SaveChanges();
        }

        public void Dispose()
        {
            this._context.Dispose();
        }
    }
}