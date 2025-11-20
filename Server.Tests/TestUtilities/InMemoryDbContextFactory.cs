using Microsoft.EntityFrameworkCore;
using Server.Data;
using System;

namespace Server.Tests.TestUtilities
{
    public static class InMemoryDbContextFactory
    {
        public static AppDbContext Create(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .EnableSensitiveDataLogging()
                .Options;

            return new AppDbContext(options);
        }
    }
}
