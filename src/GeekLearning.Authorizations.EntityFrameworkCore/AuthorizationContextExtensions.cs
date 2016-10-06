namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public static class AuthorizationContextExtensions
    {
        public static void AddAuthorizationContext(this ModelBuilder modelBuilder, string schema = null)
        {
            modelBuilder.Entity<Scope>(entity => entity.MapToTable("Scopes", schema));

            modelBuilder.Entity<ScopeHierarchy>(entity =>
            {
                entity.MapToTable("ScopeHierarchies", schema);
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

            modelBuilder.Entity<Role>(entity => entity.MapToTable("Roles"));

            modelBuilder.Entity<Right>(entity => entity.MapToTable("Rights"));

            modelBuilder.Entity<RoleRight>(entity =>
            {
                entity.MapToTable("RoleRights", schema);
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

            modelBuilder.Entity<Principal>(entity => entity.MapToTable("Principals", schema));

            modelBuilder.Entity<Authorization>(entity => entity.MapToTable("Authorizations", schema));
        }

        public static PropertyBuilder<TProperty> AddPrincipalRelationship<TProperty>(this PropertyBuilder<TProperty> propertyBuilder)
        {
            return propertyBuilder.HasAnnotation("ForeignKey", "Principal");
        }

        internal static EntityTypeBuilder<TEntity> MapToTable<TEntity>(this EntityTypeBuilder<TEntity> builder, string tableName, string schema = null)
            where TEntity : class
        {
            if (schema == null)
            {
                builder.ToTable(tableName);
            }
            else
            {
                builder.ToTable(tableName, schema);
            }

            return builder;
        }
    }
}
