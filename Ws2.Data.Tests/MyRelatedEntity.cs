using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ws2.Data.Tests;

[Table("table")]
public class MyRelatedEntity
{
	public Guid Id { get; set; }
}

public class MyRelatedEntityTypeConfiguration : IEntityTypeConfiguration<MyRelatedEntity>
{
	public void Configure(EntityTypeBuilder<MyRelatedEntity> builder)
	{
	}
}