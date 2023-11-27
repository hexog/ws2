using Microsoft.EntityFrameworkCore;

namespace Ws2.Data;

public static class DbContextDeleteExtensions
{
    public static Task DeleteAsync<TDbContext, TEntity>(
        this TDbContext context,
        TEntity entity,
        CancellationToken cancellationToken
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        context.Set<TEntity>().Remove(entity);
        return context.SaveChangesAsync(cancellationToken);
    }

    public static Task DeleteAsync<TDbContext, TEntity>(
        this TDbContext context,
        IEnumerable<TEntity> entity,
        CancellationToken cancellationToken
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        context.Set<TEntity>().RemoveRange(entity);
        return context.SaveChangesAsync(cancellationToken);
    }

    public static async Task DeleteAsync<TDbContext, TKey, TEntity>(
        this TDbContext context,
        TKey entityId,
        CancellationToken cancellationToken
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        var entity = await context.Set<TEntity>()
            .FindAsync(new object?[] { entityId }, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            return;
        }

        context.Set<TEntity>().Remove(entity);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}