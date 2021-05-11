﻿using System;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkDotNet50
{
    public class SimpleDbContextFactory
    {
        private readonly Action<DbContextOptionsBuilder> _configureDbContext;

        public SimpleDbContextFactory(Action<DbContextOptionsBuilder> configureDbContext)
        {
            _configureDbContext = configureDbContext;
        }

        public SimpleDbContext CreateDbContext()
        {
            DbContextOptionsBuilder<SimpleDbContext> options = new DbContextOptionsBuilder<SimpleDbContext>();

            _configureDbContext(options);

            return new SimpleDbContext(options.Options);
        }
    }
}
