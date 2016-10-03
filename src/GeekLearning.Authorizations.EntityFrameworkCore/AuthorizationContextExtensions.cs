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
                entity.ToTable("Rights");
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
