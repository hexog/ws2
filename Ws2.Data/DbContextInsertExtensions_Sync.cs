using Microsoft.EntityFrameworkCore;

namespace Ws2.Data;

public static partial class DbContextAddExtensions
{
    public static void AddSave<TDbContext, TEntity>(
        this TDbContext context,
        TEntity entity
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        context.Set<TEntity>().Add(entity);
        context.SaveChanges();
    }

    public static void AddSave<TDbContext, TEntity, TEntity2>(
        this TDbContext context,
        TEntity entity,
        TEntity2 entity2
    )
        where TDbContext : DbContext
        where TEntity : class
        where TEntity2 : class
    {
        context.Set<TEntity>().Add(entity);
        context.Set<TEntity2>().Add(entity2);
        context.SaveChanges();
    }

    public static void AddSave<TDbContext, TEntity, TEntity2, TEntity3>(
        this TDbContext context,
        TEntity entity,
        TEntity2 entity2,
        TEntity3 entity3
    )
        where TDbContext : DbContext
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
    {
        context.Set<TEntity>().Add(entity);
        context.Set<TEntity2>().Add(entity2);
        context.Set<TEntity3>().Add(entity3);
        context.SaveChanges();
    }

    public static void AddSave<TDbContext, TEntity>(
        this TDbContext context,
        IEnumerable<TEntity> entity
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        context.Set<TEntity>().AddRange(entity);
        context.SaveChanges();
    }
}