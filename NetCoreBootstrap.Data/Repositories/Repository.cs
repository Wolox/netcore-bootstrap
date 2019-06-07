using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NetCoreBootstrap.Data.Repositories.Database;
using NetCoreBootstrap.Data.Repositories.Interfaces;

namespace NetCoreBootstrap.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DatabaseContext _context;

        public Repository(DatabaseContext context) => _context = context;

        public DatabaseContext Context { get => _context; }

        public void Add(T entity) => _context.Set<T>().Add(entity);

        public void AddRange(IEnumerable<T> entities) => _context.Set<T>().AddRange(entities);

        public T Get(int id) => _context.Set<T>().Find(id);

        public IEnumerable<T> GetAll() => _context.Set<T>().ToList();

        public void Remove(T entity) => _context.Set<T>().Remove(entity);

        public void RemoveRange(IEnumerable<T> entities) => _context.Set<T>().RemoveRange(entities);

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate) => _context.Set<T>().Where(predicate);

        public void UpdateRange(IEnumerable<T> entities) => _context.Set<T>().UpdateRange(entities);

        public bool Update(T entity)
        {
            var updateResult = _context.Set<T>().Update(entity);
            return updateResult.State == EntityState.Modified;
        }
    }
}