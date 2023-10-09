using Microsoft.EntityFrameworkCore;

namespace Ws2.Data.Tests;

public class TestDbContext : DbContext
{
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(TestDbContext).Assembly);
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		base.OnConfiguring(optionsBuilder);
		optionsBuilder.UseInMemoryDatabase(nameof(TestDbContext));
	}
}