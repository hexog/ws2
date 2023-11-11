using Microsoft.EntityFrameworkCore;

namespace Ws2.Data;

public class EntityHandler<TEntity> where TEntity : class
{
	public DbContext DbContext { get; }

	public EntityHandler(DbContextAccessor dbContextAccessor)
	{
		DbContext = dbContextAccessor.DbContext;
	}

	public virtual ValueTask<TEntity?> FindAsync<TKey>(TKey key)
	{
		return DbContext.Set<TEntity>().FindAsync(key);
	}

	public virtual Task AddAsync(TEntity entity)
	{
		DbContext.Set<TEntity>().Add(entity);
		return DbContext.SaveChangesAsync();
	}

	public virtual Task UpdateAsync(TEntity entity)
	{
		DbContext.Set<TEntity>().Update(entity);
		return DbContext.SaveChangesAsync();
	}

	public virtual Task RemoveAsync(TEntity entity)
	{
		DbContext.Set<TEntity>().Remove(entity);
		return DbContext.SaveChangesAsync();
	}

	public IQueryable<TEntity> Query => DbContext.Set<TEntity>();
}