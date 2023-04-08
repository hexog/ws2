using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Ws2.Data;

public static class DbContextUtilities
{
	public static void AddTables(this ModelBuilder modelBuilder, params Assembly[] assemblies)
	{
		foreach (var type in assemblies.SelectMany(x => x.DefinedTypes))
		{
			var attributes = type.GetCustomAttributes(typeof(TableAttribute));
			if (attributes.Any())
			{
				modelBuilder.Entity(type);
			}
		}
	}
}