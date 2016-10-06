namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public static class AuthorizationContextExtensions
    {
        public static void AddAuthorizationContext(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Scope>(entity => entity.ToTable("Scopes"));

            modelBuilder.Entity<ScopeHierarchy>(entity =>
            {
                entity.ToTable("ScopeHierarchies");
                entity.HasKey(x => new { x.ParentId, x.ChildId });
            });

            modelBuilder.Entity<ScopeHierarchy>()
                        .HasOne(pt => pt.Parent)
                        .WithMany(p => p.Children)
                        .HasForeignKey(pt => pt.ParentId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ScopeHierarchy>()
                        .HasOne(pt => pt.Child)
                        .WithMany(t => t.Parents)
                        .HasForeignKey(pt => pt.ChildId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Role>(entity => entity.ToTable("Roles"));

            modelBuilder.Entity<Right>(entity => entity.ToTable("Rights"));

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

            modelBuilder.Entity<Principal>(entity => entity.ToTable("Principals"));

            modelBuilder.Entity<Authorization>(entity => entity.ToTable("Authorizations"));
        }

        public static PropertyBuilder<TProperty> AddPrincipalRelationship<TProperty>(this PropertyBuilder<TProperty> propertyBuilder)
        {
            return propertyBuilder.HasAnnotation("ForeignKey", "Principal");
        }
    }
}
