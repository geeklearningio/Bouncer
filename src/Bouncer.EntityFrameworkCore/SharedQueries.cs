namespace Bouncer.EntityFrameworkCore
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public static class SharedQueries
    {
        public static async Task<Data.ModelModificationDate> GetModelModificationDateAsync<TContext>(TContext context)
            where TContext : DbContext
        {
            var modelModificationDates = await context.ModelModificationDates().ToListAsync();          

            switch (modelModificationDates.Count)
            {
                case 0:
                {
                    var local = context.ChangeTracker
                        .Entries<Data.ModelModificationDate>()
                        .Select(e => e.Entity)
                        .FirstOrDefault();

                    if (local != null)
                    {
                        return local;
                    }

                    var modelModificationDate = new Data.ModelModificationDate
                    {
                        Id = 1,
                        Rights = DateTime.UtcNow,
                        Roles = DateTime.UtcNow,
                        Scopes = DateTime.UtcNow,
                    };

                    context.ModelModificationDates().Add(modelModificationDate);
                    return modelModificationDate;
                }

                case 1:
                    return modelModificationDates.First();

                default:
                {
                    var toKeep = modelModificationDates.First();
                    toKeep.Rights = modelModificationDates.Max(m => m.Rights);
                    toKeep.Roles = modelModificationDates.Max(m => m.Roles);
                    toKeep.Scopes = modelModificationDates.Max(m => m.Scopes);

                    foreach (var toDelete in modelModificationDates)
                    {
                        if (toDelete.Id == toKeep.Id)
                        {
                            continue;
                        }

                        context.ModelModificationDates().Remove(toDelete);
                    }

                    return toKeep;
                }
            }
        }
    }
}
