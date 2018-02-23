using System;

namespace NetCoreBootstrap.Persistance.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IExampleModelRepository ExampleModelRepository { get; }
        int Complete();
    }
}