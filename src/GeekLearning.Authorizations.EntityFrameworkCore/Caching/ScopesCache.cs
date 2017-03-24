namespace GeekLearning.Authorizations.EntityFrameworkCore.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ScopesCache : ICacheableObject
    {
        public IEnumerable<Scope> Scopes { get; set; }

        public string CacheKey => GetCacheKey();

        public DateTime CacheValuesDateTime { get; set; }

        internal IDictionary<Guid, Scope> Compute()
        {
            return this.Scopes.ToDictionary(r => r.Id, r => r);
        }

        public static string GetCacheKey() => nameof(ScopesCache);
    }
}
