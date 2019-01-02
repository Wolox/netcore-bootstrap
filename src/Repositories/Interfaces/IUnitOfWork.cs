using System;

namespace NetCoreBootstrap.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository UserRepository { get; }
        int Complete();
    }
}
