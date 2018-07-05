namespace GeekLearning.Bouncer.Tests
{
    using EntityFrameworkCore;
    using EntityFrameworkCore.Data;
    using Microsoft.EntityFrameworkCore;
    using System;

    public class AuthorizationsTestContext : DbContext
    {
        public Guid CurrentUserId { get; private set; }

        public AuthorizationsTestContext(DbContextOptions options) : base(options)
        {
            CurrentUserId = Guid.NewGuid();
        }

        public void Seed()
        {
            this.Set<UserTest>().Add(new UserTest { Id = CurrentUserId });

            this.Set<Principal>().Add(new Principal { CreationBy = CurrentUserId, ModificationBy = CurrentUserId, Id = CurrentUserId });

            this.SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
