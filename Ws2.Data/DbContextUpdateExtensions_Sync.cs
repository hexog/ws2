using Microsoft.EntityFrameworkCore;

namespace Ws2.Data;

public static partial class DbContextUpdateExtensions
{
    public static void UpdateSave<TDbContext, TEntity>(
        this TDbContext context,
        TEntity entity
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        context.Set<TEntity>().Update(entity);
        context.SaveChanges();
    }

    public static void UpdateSave<TDbContext, TEntity, TEntity2>(
        this TDbContext context,
        TEntity entity,
        TEntity2 entity2
    )
        where TDbContext : DbContext
        where TEntity : class
        where TEntity2 : class
    {
        context.Set<TEntity>().Update(entity);
        context.Set<TEntity2>().Update(entity2);
        context.SaveChanges();
    }

    public static void UpdateSave<TDbContext, TEntity, TEntity2, TEntity3>(
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
        context.Set<TEntity>().Update(entity);
        context.Set<TEntity2>().Update(entity2);
        context.Set<TEntity3>().Update(entity3);
        context.SaveChanges();
    }

    public static void UpdateSave<TDbContext, TEntity>(
        this TDbContext context,
        IEnumerable<TEntity> entity
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        context.Set<TEntity>().UpdateRange(entity);
        context.SaveChanges();
    }

    public static void UpdateSave<TDbContext, TEntity>(
        this TDbContext context,
        TEntity entity,
        Action<TEntity> updater
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        updater(entity);
        context.Set<TEntity>().Update(entity);
        context.SaveChanges();
    }

    public static void UpdateSave<TDbContext, TEntity>(
        this TDbContext context,
        IEnumerable<TEntity> entities,
        Action<TEntity> updater
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        var entitiesList = entities.ToList();
        foreach (var entity in entitiesList)
        {
            updater(entity);
        }

        context.Set<TEntity>().UpdateRange(entitiesList);
        context.SaveChanges();
    }

    public static void UpdateSave<TDbContext, TEntity>(
        this TDbContext context,
        TEntity entity,
        Func<TEntity, TEntity> converter
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        var newEntity = converter(entity);
        context.Set<TEntity>().Update(newEntity);
        context.SaveChanges();
    }

    public static void UpdateSave<TDbContext, TEntity>(
        this TDbContext context,
        IEnumerable<TEntity> entities,
        Func<TEntity, TEntity> converter
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        var entitiesList = entities.ToList();
        for (var i = 0; i < entitiesList.Count; i++)
        {
            entitiesList[i] = converter(entitiesList[i]);
        }

        context.Set<TEntity>().UpdateRange(entitiesList);
        context.SaveChanges();
    }
}