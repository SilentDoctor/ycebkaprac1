using Microsoft.EntityFrameworkCore;
using ProductionCaptchaSystem.Infrastructure.Interfaces;
using ProductionCaptchaSystem.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace ProductionCaptchaSystem.Infrastructure.Repositories;

public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly CaptchaDbContext Context;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(CaptchaDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual T? GetById(int id)
    {
        return DbSet.Find(id);
    }

    public virtual IEnumerable<T> GetAll()
    {
        return DbSet.ToList();
    }

    public virtual IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
    {
        return DbSet.Where(predicate).ToList();
    }

    public virtual void Add(T entity)
    {
        DbSet.Add(entity);
    }

    public virtual void AddRange(IEnumerable<T> entities)
    {
        DbSet.AddRange(entities);
    }

    public virtual void Remove(T entity)
    {
        DbSet.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        DbSet.RemoveRange(entities);
    }

    public virtual void Update(T entity)
    {
        DbSet.Update(entity);
    }
}