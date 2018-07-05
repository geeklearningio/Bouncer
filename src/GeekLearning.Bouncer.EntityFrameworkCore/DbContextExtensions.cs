namespace GeekLearning.Bouncer.EntityFrameworkCore
{
    using GeekLearning.Bouncer.EntityFrameworkCore.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public static class DbContextExtensions
    {
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

        public static DbSet<Group> Groups<TContext>(this TContext context)
           where TContext : DbContext
        {
            return context.Set<Group>();
        }

        public static DbSet<Membership> Memberships<TContext>(this TContext context)
           where TContext : DbContext
        {
            return context.Set<Membership>();
        }

        public static DbSet<ScopeHierarchy> ScopeHierarchies<TContext>(this TContext context)
           where TContext : DbContext
        {
            return context.Set<ScopeHierarchy>();
        }

        public static DbSet<ModelModificationDate> ModelModificationDates<TContext>(this TContext context)
            where TContext : DbContext
        {
            return context.Set<ModelModificationDate>();
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
