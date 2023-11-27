﻿using Microsoft.EntityFrameworkCore;

namespace Ws2.Data.EntityHandlers;

public class DbContextAccessor
{
	public DbContextAccessor(DbContext dbContext)
	{
		DbContext = dbContext;
	}

	public DbContext DbContext { get; }
}