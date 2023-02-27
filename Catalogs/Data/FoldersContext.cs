using System;
using Catalogs.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalogs.Data
{
	public class FoldersContext : DbContext
	{
		public FoldersContext(DbContextOptions<FoldersContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Folder> Folders { get; set; } = null!;
	}
}

