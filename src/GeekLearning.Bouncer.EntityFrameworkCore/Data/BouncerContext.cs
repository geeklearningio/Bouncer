namespace GeekLearning.Bouncer.EntityFrameworkCore.Data
{
    using GeekLearning.Bouncer.EntityFrameworkCore.Configuration;
    using GeekLearning.Bouncer.EntityFrameworkCore.Data.Extensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.Options;

    public class BouncerContext : DbContext
    {
        public BouncerContext(DbContextOptions<BouncerContext> options)
               : base(options)
        {
        }

        public DbSet<Scope> Scopes { get; set; }

        public DbSet<ScopeHierarchy> ScopeHierarchies { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<Right> Rights { get; set; }

        public DbSet<RoleRight> RoleRights { get; set; }

        public DbSet<Principal> Principals { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<Membership> Memberships { get; set; }

        public DbSet<Authorization> Authorizations { get; set; }

        public DbSet<ModelModificationDate> ModelModificationDates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            BouncerContextOptions options = this.GetService<IOptionsSnapshot<BouncerContextOptions>>().Value;
     
            var schemaName = options?.SchemaName;
            if (string.IsNullOrEmpty(schemaName))
            {
                // Force null value for empty values
                schemaName = null;
            }

            modelBuilder.Entity<Scope>(entity =>
            {
                entity.MapToTable("Scope", schemaName)
                      .HasIndex(s => s.Name).IsUnique();
            });

            modelBuilder.Entity<ScopeHierarchy>(entity =>
            {
                entity.MapToTable("ScopeHierarchy", schemaName)
                      .HasKey(sh => new { sh.ParentId, sh.ChildId });
                entity.HasOne(sh => sh.Parent)
                      .WithMany(s => s.Children)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.MapToTable("Role", schemaName)
                      .HasIndex(r => r.Name).IsUnique();
            });

            modelBuilder.Entity<Right>(entity =>
            {
                entity.MapToTable("Right", schemaName)
                      .HasIndex(r => r.Name).IsUnique();
            });

            modelBuilder.Entity<RoleRight>(entity =>
            {
                entity.MapToTable("RoleRight", schemaName)
                      .HasKey(rr => new { rr.RoleId, rr.RightId });
            });

            modelBuilder.Entity<Principal>(entity =>
            {
                entity.MapToTable("Principal", schemaName);
                entity.HasOne(p => p.Group)
                      .WithOne(b => b.AssociatedPrincipal)
                      .HasForeignKey<Group>(g => g.Id);
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.MapToTable("Group", schemaName)
                      .HasIndex(g => g.Name)
                      .IsUnique();           
            });
            
            modelBuilder.Entity<Membership>(entity =>
            {
                entity.MapToTable("Membership", schemaName)                      
                      .HasKey(ms => new { ms.GroupId, ms.PrincipalId });
                entity.HasOne(ms => ms.Group)
                      .WithMany(g => g.Members)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Authorization>(entity =>
            {
                entity.MapToTable("Authorization", schemaName)
                      .HasIndex(a => new { a.RoleId, a.ScopeId, a.PrincipalId }).IsUnique();
            });

            modelBuilder.Entity<ModelModificationDate>(entity =>
            {
                entity.MapToTable("ModelModificationDate", schemaName);
            });
        }
    }
}
