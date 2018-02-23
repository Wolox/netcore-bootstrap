// TODO DELETE THIS FILE
using Microsoft.EntityFrameworkCore;
using NetCoreBootstrap.Models.Database;
using NetCoreBootstrap.Persistance.Interfaces;
using NetCoreBootstrap.Persistance.Repositories;

namespace src.Persistance.Repositories
{
    public class ExampleModelRepository : Repository<ExampleModel>, IExampleModelRepository
    {
        public ExampleModelRepository(DbContext context) : base(context)
        {
        }
    }
}