using System;

namespace NetCoreBootstrap.Data.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository UserRepository { get; }
        int Complete();
    }
}