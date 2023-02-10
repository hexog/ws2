using Microsoft.EntityFrameworkCore;

namespace Ws2.Data;

public class EntityHandler<TEntity> where TEntity : class
{
	private readonly DbContext dbContext;

	public EntityHandler(DbContextAccessor dbContextAccessor)
	{
		dbContext = dbContextAccessor.DbContext;
	}

	public virtual ValueTask<TEntity?> FindAsync<TKey>(TKey key)
	{
		return dbContext.Set<TEntity>().FindAsync(key);
	}

	public virtual Task AddAsync(TEntity entity)
	{
		dbContext.Set<TEntity>().Add(entity);
		return dbContext.SaveChangesAsync();
	}

	public virtual Task UpdateAsync(TEntity entity)
	{
		dbContext.Set<TEntity>().Update(entity);
		return dbContext.SaveChangesAsync();
	}

	public virtual Task RemoveAsync(TEntity entity)
	{
		dbContext.Set<TEntity>().Remove(entity);
		return dbContext.SaveChangesAsync();
	}

	public IQueryable<TEntity> Query => dbContext.Set<TEntity>();
}