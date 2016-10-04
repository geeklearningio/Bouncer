namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public static class AuthorizationContextExtensions
    {
        public static void AddAuthorizationContext(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Scope>(entity =>
            {
                entity.ToTable("Scopes");
                entity.Property(e => e.IsDeletable).HasDefaultValue(true);
                entity.Property(e => e.Id).HasDefaultValueSql("newid()");
                entity.Property(e => e.CreationDate).HasDefaultValueSql("getutcdate()");
                entity.Property(e => e.ModificationDate).HasDefaultValueSql("getutcdate()");
            });

            modelBuilder.Entity<ScopeHierarchy>(entity =>
            {
                entity.ToTable("ScopeHierarchies");
                entity.HasKey(x => new { x.ParentId, x.ChildId });
            });

            modelBuilder.Entity<ScopeHierarchy>()
                        .HasOne(pt => pt.Parent)
                        .WithMany(p => p.Children)
                        .HasForeignKey(pt => pt.ParentId);

            modelBuilder.Entity<ScopeHierarchy>()
                        .HasOne(pt => pt.Child)
                        .WithMany(t => t.Parents)
                        .HasForeignKey(pt => pt.ChildId);

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.Property(e => e.IsDeletable).HasDefaultValue(true);
                entity.Property(e => e.Id).HasDefaultValueSql("newid()");
                entity.Property(e => e.CreationDate).HasDefaultValueSql("getutcdate()");
                entity.Property(e => e.ModificationDate).HasDefaultValueSql("getutcdate()");
            });

            modelBuilder.Entity<Right>(entity =>
            {
                entity.ToTable("Right");
                entity.Property(e => e.IsDeletable).HasDefaultValue(true);
                entity.Property(e => e.Id).HasDefaultValueSql("newid()");
                entity.Property(e => e.CreationDate).HasDefaultValueSql("getutcdate()");
                entity.Property(e => e.ModificationDate).HasDefaultValueSql("getutcdate()");
            });

            modelBuilder.Entity<RoleRight>(entity =>
            {
                entity.ToTable("RoleRights");
                entity.HasKey(x => new { x.RoleId, x.RightId });
            });

            modelBuilder.Entity<RoleRight>()
                        .HasOne(pt => pt.Role)
                        .WithMany(p => p.Rights)
                        .HasForeignKey(pt => pt.RoleId);

            modelBuilder.Entity<RoleRight>()
                        .HasOne(pt => pt.Right)
                        .WithMany(t => t.Roles)
                        .HasForeignKey(pt => pt.RightId);

            modelBuilder.Entity<Principal>(entity =>
            {
                entity.ToTable("Principals");
                entity.Property(e => e.IsDeletable).HasDefaultValue(true);
                entity.Property(e => e.CreationDate).HasDefaultValueSql("getutcdate()");
                entity.Property(e => e.ModificationDate).HasDefaultValueSql("getutcdate()");
            });

            modelBuilder.Entity<Authorization>(entity =>
            {
                entity.ToTable("Authorizations");
                entity.Property(e => e.IsDeletable).HasDefaultValue(true);
                entity.Property(e => e.Id).HasDefaultValueSql("newid()");
                entity.Property(e => e.CreationDate).HasDefaultValueSql("getutcdate()");
                entity.Property(e => e.ModificationDate).HasDefaultValueSql("getutcdate()");
            });
        }

        public static PropertyBuilder<TProperty> AddPrincipalRelationship<TProperty>(this PropertyBuilder<TProperty> propertyBuilder)
        {
            return propertyBuilder.HasAnnotation("ForeignKey", "Principal");
        }
    }
}
