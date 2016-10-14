// http://stackoverflow.com/questions/29030472/dbset-doesnt-have-a-find-method-in-ef7

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Microsoft.Data.Entity.Extensions
{
    public static class Extensions
    {
        public static async Task<TEntity> FindAsync<TEntity, TContext>(this DbSet<TEntity> set, TContext context, params object[] keyValues)
            where TEntity : class
            where TContext : DbContext
        {
            var entityType = context.Model.FindEntityType(typeof(TEntity));
            var key = entityType.FindPrimaryKey();

            var entries = context.ChangeTracker.Entries<TEntity>();

            var i = 0;
            foreach (var property in key.Properties)
            {
                var i1 = i;
                entries = entries.Where(e => e.Property(property.Name).CurrentValue == keyValues[i1]);
                i++;
            }

            var entry = entries.FirstOrDefault();
            if (entry != null)
            {
                // Return the local object if it exists.
                return entry.Entity;
            }

            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var query = set.AsQueryable();
            i = 0;
            foreach (var property in key.Properties)
            {
                var i1 = i;
                query = query.Where((Expression<Func<TEntity, bool>>)
                 Expression.Lambda(
                     Expression.Equal(
                         Expression.Property(parameter, property.Name),
                         Expression.Constant(keyValues[i1])),
                     parameter));
                i++;
            }

            // Look in the database
            return await query.FirstOrDefaultAsync();
        }
    }
}