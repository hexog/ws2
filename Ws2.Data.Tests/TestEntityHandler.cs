using Microsoft.EntityFrameworkCore;
using Ws2.Data.EntityHandlers;

namespace Ws2.Data.Tests;

public class TestEntityHandler
{
	private TestDbContext testDbContext = null!;
	private readonly Guid relatedEntityId1 = Guid.NewGuid();
	private readonly Guid entityId = Guid.NewGuid();
	private EntityHandler<MyEntity> entityHandler = null!;
	private MyEntity entity = null!;

	[OneTimeSetUp]
	public void OneTimeSetup()
	{
		testDbContext = new TestDbContext();
		testDbContext.Set<MyRelatedEntity>()
			.AddRange(
				new MyRelatedEntity { Id = relatedEntityId1 }
			);

		entity = new MyEntity
		{
			Id = entityId,
			MyRelatedEntityId = relatedEntityId1,
			MyString = "AAA",
			MyDateTime = null
		};

		testDbContext.Set<MyEntity>()
			.AddRange(
				entity
			);

		testDbContext.SaveChanges();
	}

	[OneTimeTearDown]
	public void OneTimeTearDown()
	{
		testDbContext.Dispose();
	}

	[SetUp]
	public void Setup()
	{
		entityHandler = new EntityHandler<MyEntity>(new DbContextAccessor(testDbContext));
	}

	[Test]
	public async Task TestFind()
	{
		var actual = await entityHandler.FindAsync(entityId);
		Assert.That(
			entity.Id == actual!.Id
			&& entity.MyString == actual.MyString
			&& entity.MyRelatedEntity == actual.MyRelatedEntity
			&& entity.MyDateTime == actual.MyDateTime
		);
	}

	[Test]
	public async Task TestAdd()
	{
		var newEntity = new MyEntity
		{
			Id = Guid.NewGuid(),
			MyString = "KBKBK",
			MyRelatedEntityId = relatedEntityId1
		};
		await entityHandler.AddAsync(newEntity);
		var actual = await entityHandler.FindAsync(newEntity.Id);

		Assert.That(actual, Is.SameAs(newEntity));
	}

	[Test]
	public async Task TestUpdate()
	{
		var updateEntityId = Guid.NewGuid();
		await entityHandler.AddAsync(new MyEntity
			{ Id = updateEntityId, MyString = "XXX", MyRelatedEntityId = relatedEntityId1 });

		var updateEntity = await entityHandler.FindAsync(updateEntityId);
		updateEntity!.MyDateTime = new DateTime(2020, 1, 3, 4, 5, 6);

		await entityHandler.UpdateAsync(updateEntity);

		var actual = await entityHandler.FindAsync(updateEntityId);

		Assert.That(actual!.MyString, Is.EqualTo(updateEntity.MyString));
		Assert.That(actual.MyDateTime, Is.EqualTo(updateEntity.MyDateTime));
	}

	[Test]
	public async Task TestDelete()
	{
		var deleteEntity = new MyEntity { Id = Guid.NewGuid(), MyString = "XXX", MyRelatedEntityId = relatedEntityId1 };
		await entityHandler.AddAsync(deleteEntity);

		var found = await entityHandler.FindAsync(deleteEntity.Id);
		Assert.That(found, Is.Not.Null);

		await entityHandler.RemoveAsync(found!);

		found = await entityHandler.FindAsync(deleteEntity.Id);
		Assert.That(found, Is.Null);
	}

	[Test]
	public async Task TestInclude()
	{
		var actual = await entityHandler.Query.Include(x => x.MyRelatedEntity).FirstOrDefaultAsync();

		Assert.That(actual, Is.Not.Null);
#pragma warning disable NUnit2045
		Assert.That(actual!.MyRelatedEntity, Is.Not.Null);
#pragma warning restore NUnit2045
		Assert.That(actual.MyRelatedEntity.Id, Is.EqualTo(relatedEntityId1));
	}
}