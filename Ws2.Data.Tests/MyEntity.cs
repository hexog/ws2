using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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