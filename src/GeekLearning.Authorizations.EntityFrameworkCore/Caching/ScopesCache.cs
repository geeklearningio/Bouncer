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

        internal IDictionary<TKey, Scope> Compute<TKey>(Func<Scope, TKey> keySelector)
        {
            return this.Scopes.ToDictionary(keySelector, r => r);
        }

        public static string GetCacheKey() => nameof(ScopesCache);
    }
}
