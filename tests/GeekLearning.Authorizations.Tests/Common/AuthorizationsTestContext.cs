namespace GeekLearning.Authorizations.Tests
{
    using Microsoft.EntityFrameworkCore;
    using GeekLearning.Authorizations.EntityFrameworkCore;
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

            this.Set<Data.Principal>().Add(new Data.Principal { Id = CurrentUserId });

            this.SaveChanges();
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
