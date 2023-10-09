using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ws2.Data.Tests;

[Table("MyTable")]
public class MyEntity
{
	[Key]
	public Guid Id { get; set; }

	[Required]
	public string MyString { get; set; } = null!;

	[Column("MyDateTime2")]
	public DateTime? MyDateTime { get; set; }

	[Required]
	public Guid MyRelatedEntityId { get; set; }

	public MyRelatedEntity MyRelatedEntity { get; set; } = null!;
}

public class MyEntityTypeConfiguration : IEntityTypeConfiguration<MyEntity>
{
	public void Configure(EntityTypeBuilder<MyEntity> builder)
	{
	}
}