using Microsoft.EntityFrameworkCore;

namespace Ws2.Data;

public static partial class DbContextUpdateExtensions
{
    public static Task UpdateSaveAsync<TDbContext, TEntity>(
        this TDbContext context,
        TEntity entity,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        context.Set<TEntity>().Update(entity);
        return context.SaveChangesAsync(cancellationToken);
    }

    public static Task UpdateSaveAsync<TDbContext, TEntity, TEntity2>(
        this TDbContext context,
        TEntity entity,
        TEntity2 entity2,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TEntity : class
        where TEntity2 : class
    {
        context.Set<TEntity>().Update(entity);
        context.Set<TEntity2>().Update(entity2);
        return context.SaveChangesAsync(cancellationToken);
    }

    public static Task UpdateSaveAsync<TDbContext, TEntity, TEntity2, TEntity3>(
        this TDbContext context,
        TEntity entity,
        TEntity2 entity2,
        TEntity3 entity3,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
    {
        context.Set<TEntity>().Update(entity);
        context.Set<TEntity2>().Update(entity2);
        context.Set<TEntity3>().Update(entity3);
        return context.SaveChangesAsync(cancellationToken);
    }

    public static Task UpdateSaveAsync<TDbContext, TEntity>(
        this TDbContext context,
        IEnumerable<TEntity> entity,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        context.Set<TEntity>().UpdateRange(entity);
        return context.SaveChangesAsync(cancellationToken);
    }

    public static Task UpdateSaveAsync<TDbContext, TEntity>(
        this TDbContext context,
        TEntity entity,
        Action<TEntity> updater,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        updater(entity);
        context.Set<TEntity>().Update(entity);
        return context.SaveChangesAsync(cancellationToken);
    }

    public static async Task UpdateSaveAsync<TDbContext, TEntity>(
        this TDbContext context,
        TEntity entity,
        Func<TEntity, Task> updater,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        await updater(entity);
        context.Set<TEntity>().Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public static Task UpdateSaveAsync<TDbContext, TEntity>(
        this TDbContext context,
        IEnumerable<TEntity> entities,
        Action<TEntity> updater,
        CancellationToken cancellationToken = default
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
        return context.SaveChangesAsync(cancellationToken);
    }

    public static async Task UpdateSaveAsync<TDbContext, TEntity>(
        this TDbContext context,
        IEnumerable<TEntity> entities,
        Func<TEntity, Task> updater,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        var entitiesList = entities.ToList();
        foreach (var entity in entitiesList)
        {
            await updater(entity);
        }

        context.Set<TEntity>().UpdateRange(entitiesList);
        await context.SaveChangesAsync(cancellationToken);
    }


    public static Task UpdateSaveAsync<TDbContext, TEntity>(
        this TDbContext context,
        TEntity entity,
        Func<TEntity, TEntity> converter,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        var newEntity = converter(entity);
        context.Set<TEntity>().Update(newEntity);
        return context.SaveChangesAsync(cancellationToken);
    }

    public static Task UpdateSaveAsync<TDbContext, TEntity>(
        this TDbContext context,
        IEnumerable<TEntity> entities,
        Func<TEntity, TEntity> converter,
        CancellationToken cancellationToken = default
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
        return context.SaveChangesAsync(cancellationToken);
    }

    public static async Task UpdateSaveAsync<TDbContext, TEntity>(
        this TDbContext context,
        TEntity entity,
        Func<TEntity, Task<TEntity>> converter,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        var newEntity = await converter(entity);
        context.Set<TEntity>().Update(newEntity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public static async Task UpdateSaveAsync<TDbContext, TEntity>(
        this TDbContext context,
        IEnumerable<TEntity> entities,
        Func<TEntity, Task<TEntity>> converter,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        var entitiesList = entities.ToList();
        for (var i = 0; i < entitiesList.Count; i++)
        {
            entitiesList[i] = await converter(entitiesList[i]);
        }

        context.Set<TEntity>().UpdateRange(entitiesList);
        await context.SaveChangesAsync(cancellationToken);
    }
}