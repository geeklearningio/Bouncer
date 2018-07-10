namespace GeekLearning.Bouncer.EntityFrameworkCore.Data.Extensions
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public static class DbContextExtensions
    {
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
