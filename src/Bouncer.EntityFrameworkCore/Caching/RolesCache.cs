namespace Bouncer.EntityFrameworkCore.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class RolesCache : ICacheableObject
    {
        public IEnumerable<Role> Roles { get; set; }

        public string CacheKey => GetCacheKey();

        public DateTime CacheValuesDateTime { get; set; }

        internal IDictionary<Guid, Role> Compute()
        {
            return this.Roles.ToDictionary(r => r.Id, r => r);
        }

        public static string GetCacheKey() => nameof(RolesCache);
    }
}
