namespace Samples.ConsoleApp
{
    using Bouncer.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using System;

    class Program
    {
        static void Main(string[] args)
        {
        }
    }

    public class SampleDbContext : DbContext
    {
        private readonly string connectionString = "Server=(localdb)\\mssqllocaldb;Database=Test;Trusted_Connection=True;MultipleActiveResultSets=true";

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddAuthorizationContext(new ModelBuilderOptions { SchemaName = "Authorizations" });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
