using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MyMVCProject.Models.Database;

namespace MyMVCProject.Repositories
{
    public abstract class MasterRepository <T> where T: BaseEntity
    {
        private readonly DbContextOptions<DataBaseContext> _options;

        public MasterRepository(DbContextOptions<DataBaseContext> options)
        {
            this._options = options;
        }

        public T GetById(int id)
        {
            using(var context = Context)
            {
                return context.Set<T>().Find(id);
            }
        }

        public List<T> GetAll()
        {
            using(var context = Context)
            {
                return context.Set<T>().ToList();
            }
        }

        public void Insert(T entity)
        {
            using(var context = Context)
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("entity");
                }
                entity.CreateAt = DateTime.Now;
                entity.UpdateAt = DateTime.Now;
                context.Set<T>().Add(entity);
                context.SaveChanges();
            }
        }

        public void Update(T entity)
        {
            using(var context = Context)
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("entity");
                }
                entity.UpdateAt = DateTime.Now;
                context.Set<T>().Update(entity);
                context.SaveChanges();
            }
        }

        public void Delete(T entity)
        {
            using(var context = Context)
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("entity");
                }
                context.Set<T>().Remove(entity);
                context.SaveChanges();
            }
        }

        public DbContextOptions<DataBaseContext> Options
        {
            get {return _options;}
        }

        public DataBaseContext Context
        {
            get {return new DataBaseContext(Options);}
        }
    }
}
