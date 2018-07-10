namespace GeekLearning.Bouncer.Tests
{
    using EntityFrameworkCore.Data;
    using Microsoft.EntityFrameworkCore;
    using System;

    public class BouncerTestContext : BouncerContext
    {
        public Guid CurrentUserId { get; private set; }

        public BouncerTestContext(DbContextOptions<BouncerContext> options) : base(options)
        {
            CurrentUserId = Guid.NewGuid();
        }

        public void Seed()
        {
            this.Set<Principal>().Add(new Principal { CreationBy = CurrentUserId, ModificationBy = CurrentUserId, Id = CurrentUserId });

            this.SaveChanges();
        }
    }
}
