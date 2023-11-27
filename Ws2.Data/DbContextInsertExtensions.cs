using Microsoft.EntityFrameworkCore;

namespace Ws2.Data;

public static class DbContextInsertExtensions
{
    public static Task InsertAsync<TDbContext, TEntity>(
        this TDbContext context,
        TEntity entity,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        context.Set<TEntity>().Add(entity);
        return context.SaveChangesAsync(cancellationToken);
    }

    public static Task InsertAsync<TDbContext, TEntity>(
        this TDbContext context,
        IEnumerable<TEntity> entity,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TEntity : class
    {
        context.Set<TEntity>().AddRange(entity);
        return context.SaveChangesAsync(cancellationToken);
    }
}