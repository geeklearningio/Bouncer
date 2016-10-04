using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using GeekLearning.Authorizations.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeekLearning.Authorizations.Tests
{
    public class AuthorizationsTestContext : DbContext
    {
        public Guid CurrentUserId => Guid.NewGuid();

        public void Seed()
        {
            this.Set<UserTest>().Add(new UserTest
            {
                Id = CurrentUserId
            });

            this.SaveChanges();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);

            optionsBuilder.UseSqlite(connection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddAuthorizationContext();

            modelBuilder.Entity<UserTest>(entity =>
            {
                entity.ToTable("UserTest");
                entity.Property(e => e.Id)
                      .HasDefaultValueSql("newid()")
                      .AddPrincipalRelationship();
            });
        }
    }
}
