using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ws2.Data.Tests;

[Table("table")]
public class TestSimpleEntity
{
    public Guid Id { get; set; }

    public int Int { get; set; }
}

public class MyRelatedEntityTypeConfiguration : IEntityTypeConfiguration<TestSimpleEntity>
{
    public void Configure(EntityTypeBuilder<TestSimpleEntity> builder)
    {
    }
}