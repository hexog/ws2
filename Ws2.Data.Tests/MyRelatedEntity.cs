using System.ComponentModel.DataAnnotations.Schema;

namespace Ws2.Data.Tests;

[Table("table")]
public class MyRelatedEntity
{
	public Guid Id { get; set; }
}