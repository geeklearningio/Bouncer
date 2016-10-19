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
            modelBuilder.Entity<Scope>(entity =>
            {
                entity.MapToTable("Scope", schema);
                entity.HasIndex(s => s.Name).IsUnique();
            });

            modelBuilder.Entity<ScopeHierarchy>(entity =>
            {
                entity.MapToTable("ScopeHierarchy", schema);
                entity.HasKey(sh => new { sh.ParentId, sh.ChildId });
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

            modelBuilder.Entity<Role>(entity =>
            {
                entity.MapToTable("Role", schema);
                entity.HasIndex(r => r.Name).IsUnique();
            });

            modelBuilder.Entity<Right>(entity =>
            {
                entity.MapToTable("Right", schema);
                entity.HasIndex(r => r.Name).IsUnique();
            });

            modelBuilder.Entity<RoleRight>(entity =>
            {
                entity.MapToTable("RoleRight", schema);
                entity.HasKey(rr => new { rr.RoleId, rr.RightId });
            });

            modelBuilder.Entity<Principal>(entity => entity.MapToTable("Principal", schema));

            modelBuilder.Entity<Authorization>(entity =>
            {
                entity.MapToTable("Authorization", schema);
                entity.HasIndex(a => new { a.RoleId, a.ScopeId, a.PrincipalId }).IsUnique();
            });
        }

        public static DbSet<Authorization> Authorizations<TContext>(this TContext context)
            where TContext : DbContext
        {
            return context.Set<Authorization>();
        }

        public static DbSet<Principal> Principals<TContext>(this TContext context)
            where TContext : DbContext
        {
            return context.Set<Principal>();
        }

        public static DbSet<Right> Rights<TContext>(this TContext context)
            where TContext : DbContext
        {
            return context.Set<Right>();
        }

        public static DbSet<Role> Roles<TContext>(this TContext context)
            where TContext : DbContext
        {
            return context.Set<Role>();
        }

        public static DbSet<RoleRight> RoleRights<TContext>(this TContext context)
            where TContext : DbContext
        {
            return context.Set<RoleRight>();
        }

        public static DbSet<Scope> Scopes<TContext>(this TContext context)
           where TContext : DbContext
        {
            return context.Set<Scope>();
        }

        public static DbSet<ScopeHierarchy> ScopeHierarchies<TContext>(this TContext context)
           where TContext : DbContext
        {
            return context.Set<ScopeHierarchy>();
        }

        public static PropertyBuilder<TProperty> AddPrincipalRelationship<TProperty>(this PropertyBuilder<TProperty> propertyBuilder)
        {
            return propertyBuilder.HasAnnotation("ForeignKey", "Principal");
        }

        private static EntityTypeBuilder<TEntity> MapToTable<TEntity>(this EntityTypeBuilder<TEntity> builder, string tableName, string schema = null)
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
