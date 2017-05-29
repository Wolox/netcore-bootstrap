
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MyMVCProject.Models.Database;

namespace MyMVCProject.Respositories
{
    public abstract class MasterRepository <T> where T: BaseEntity
    {
        public T Get(long id)
        {
            using(var context = new DataBaseContext())
            {
                return context.Set<T>().Find(id);
            }
        }

        public List<T> GetAll(long id)
        {
            using(var context = new DataBaseContext())
            {
                return context.Set<T>().ToList();
            }
        }

        public void Insert(T entity)
        {
            using(var context = new DataBaseContext())
            {
                if (entity == null) {
                    throw new ArgumentNullException("entity");
                }
                context.Set<T>().Add(entity);
                context.SaveChanges();
            }
        }

        public void Update(T entity)
        {
            using(var context = new DataBaseContext())
            {
                if (entity == null) {
                    throw new ArgumentNullException("entity");
                }
                context.SaveChanges();
            }
        }

        public void Delete(T entity)
        {
            using(var context = new DataBaseContext())
            {
                if (entity == null) {
                    throw new ArgumentNullException("entity");
                }
                context.Set<T>().Remove(entity);
                context.SaveChanges();
            }
        }
    }
}
