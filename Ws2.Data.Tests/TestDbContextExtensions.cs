using FluentAssertions;

namespace Ws2.Data.Tests;

public class TestDbContextExtensions
{
    private TestDbContext dbContext = null!;

    [SetUp]
    public void Setup()
    {
        dbContext = new TestDbContext();
    }

    [TearDown]
    public void TearDown()
    {
        dbContext.Dispose();
    }

    [Test]
    public void TestInsert()
    {
        var testEntity = new TestSimpleEntity();
        dbContext.AddSave(testEntity);
        testEntity.Id.Should().NotBeEmpty();
    }

    [Test]
    public void TestInsertDifferentEntities()
    {
        var testEntity = new TestSimpleEntity();
        var testEntity2 = new TestComplexEntity { SimpleEntity = testEntity, String = "123", DateTime = DateTime.Now };
        dbContext.AddSave(testEntity, testEntity2);
        testEntity.Id.Should().NotBeEmpty();
    }

    [Test]
    public void TestInsertMultipleEntitiesOfSameType()
    {
        var testEntity = new TestSimpleEntity();
        var testEntity2 = new TestSimpleEntity();
        dbContext.AddSave(testEntity, testEntity2);
        testEntity.Id.Should().NotBeEmpty();
    }

    [Test]
    public void TestUpdate()
    {
        var complexEntity = new TestComplexEntity { String = "123", DateTime = DateTime.Now };
        dbContext.AddSave(complexEntity);

        complexEntity.String = "321";
        dbContext.Update(complexEntity);

        var testComplexEntity = dbContext.Set<TestComplexEntity>().FirstOrDefault(x => x.Id == complexEntity.Id);
        testComplexEntity.Should().NotBeNull();
        testComplexEntity!.String.Should().Be("321");
    }

    [Test]
    public void TestUpdateDifferentEntities()
    {
        var complexEntity = new TestComplexEntity { String = "123", DateTime = DateTime.Now };
        var simpleEntity = new TestSimpleEntity();
        dbContext.AddSave(complexEntity, simpleEntity);

        complexEntity.String = "333";
        simpleEntity.Int = 3333;

        var testComplexEntity = dbContext.Set<TestComplexEntity>().First(x => x.Id == complexEntity.Id);
        testComplexEntity.String.Should().Be("333");
        var testSimpleEntity = dbContext.Set<TestSimpleEntity>().First(x => x.Id == simpleEntity.Id);
        testSimpleEntity.Int.Should().Be(3333);
    }

    [Test]
    public void TestDelete()
    {
        var complexEntity = new TestComplexEntity { String = "123", DateTime = DateTime.Now };
        dbContext.AddSave(complexEntity);

        _ = dbContext.Set<TestComplexEntity>().FirstOrDefault(x => x.Id == complexEntity.Id);

        dbContext.RemoveSave(complexEntity);

        var testComplexEntity = dbContext.Set<TestComplexEntity>().FirstOrDefault(x => x.Id == complexEntity.Id);
        testComplexEntity.Should().BeNull();
    }

    [Test]
    public void TestDeleteDifferentEntities()
    {
        var complexEntity = new TestComplexEntity { String = "123", DateTime = DateTime.Now };
        var simpleEntity = new TestSimpleEntity();
        dbContext.AddSave(complexEntity, simpleEntity);

        _ = dbContext.Set<TestComplexEntity>().First(x => x.Id == complexEntity.Id);
        _ = dbContext.Set<TestSimpleEntity>().First(x => x.Id == simpleEntity.Id);

        dbContext.RemoveSave(complexEntity, simpleEntity);

        dbContext.Set<TestComplexEntity>().FirstOrDefault(x => x.Id == complexEntity.Id)
            .Should().BeNull();
        dbContext.Set<TestSimpleEntity>().FirstOrDefault(x => x.Id == simpleEntity.Id)
            .Should().BeNull();
    }

}