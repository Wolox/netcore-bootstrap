using System;

namespace NetCoreBootstrap.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        int Complete();
    }
}
