using System;

namespace NetCoreBootstrap.Persistance.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        int Complete();
    }
}