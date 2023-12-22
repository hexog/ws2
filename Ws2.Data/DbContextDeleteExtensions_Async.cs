using Microsoft.EntityFrameworkCore;

namespace Ws2.Data;

public static partial class DbContextDeleteExtensions
{
    public static Task RemoveSaveAsync<TDbContext, TEntity>(
        this TDbContext context,
        TEntity entity,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        context.Set<TEntity>().Remove(entity);
        return context.SaveChangesAsync(cancellationToken);
    }

    public static Task RemoveSaveAsync<TDbContext, TEntity, TEntity2>(
        this TDbContext context,
        TEntity entity,
        TEntity2 entity2,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TEntity : class
        where TEntity2 : class
    {
        context.Set<TEntity>().Remove(entity);
        context.Set<TEntity2>().Remove(entity2);
        return context.SaveChangesAsync(cancellationToken);
    }

    public static Task RemoveSaveAsync<TDbContext, TEntity, TEntity2, TEntity3>(
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
        context.Set<TEntity>().Remove(entity);
        context.Set<TEntity2>().Remove(entity2);
        context.Set<TEntity3>().Remove(entity3);
        return context.SaveChangesAsync(cancellationToken);
    }

    public static Task RemoveSaveAsync<TDbContext, TEntity>(
        this TDbContext context,
        IEnumerable<TEntity> entity,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        context.Set<TEntity>().RemoveRange(entity);
        return context.SaveChangesAsync(cancellationToken);
    }
}