using Microsoft.EntityFrameworkCore;

namespace Ws2.Data;

public static class DbContextUpdateExtensions
{
    public static Task UpdateAsync<TDbContext, TEntity>(
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

    public static Task UpdateAsync<TDbContext, TEntity>(
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

    public static Task UpdateAsync<TDbContext, TEntity>(
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

    public static Task UpdateAsync<TDbContext, TEntity>(
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

    public static Task UpdateAsync<TDbContext, TEntity>(
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

    public static Task UpdateAsync<TDbContext, TEntity>(
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

    public static async Task UpdateAsync<TDbContext, TKey, TEntity>(
        this TDbContext context,
        TKey entityId,
        Action<TEntity> updater,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        var set = context.Set<TEntity>();
        var entity = await set.FindAsync(new object?[] { entityId }, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (entity is null)
        {
            return;
        }

        updater(entity);
        set.Update(entity);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async Task UpdateAsync<TDbContext, TKey, TEntity>(
        this TDbContext context,
        TKey entityId,
        Func<TEntity, TEntity> converter,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        var set = context.Set<TEntity>();
        var entity = await set.FindAsync(new object?[] { entityId }, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (entity is null)
        {
            return;
        }

        var newEntity = converter(entity);
        context.Set<TEntity>().Update(newEntity);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}