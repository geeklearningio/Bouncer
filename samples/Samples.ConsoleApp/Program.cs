namespace Samples.ConsoleApp
{
    using Microsoft.EntityFrameworkCore;

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
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
