using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ws2.Data.Tests;

public class TestComplexEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string String { get; set; } = null!;

    [Column("DateTime2")]
    public DateTime? DateTime { get; set; }

    public Guid? RelatedEntityId { get; set; }

    public TestSimpleEntity? SimpleEntity { get; set; } = null!;
}

public class MyEntityTypeConfiguration : IEntityTypeConfiguration<TestComplexEntity>
{
    public void Configure(EntityTypeBuilder<TestComplexEntity> builder)
    {
    }
}