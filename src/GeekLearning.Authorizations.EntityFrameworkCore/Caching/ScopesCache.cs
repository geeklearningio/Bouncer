namespace GeekLearning.Authorizations.EntityFrameworkCore.Caching
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ScopesCache : ICacheableObject
    {
        [JsonProperty("s")]
        public IEnumerable<Scope> Scopes { get; set; }

        public string CacheKey => GetCacheKey();

        internal IDictionary<Guid, Scope> Compute()
        {
            return this.Scopes.ToDictionary(r => r.Id, r => r);
        }

        public static string GetCacheKey() => nameof(ScopesCache);
    }
}
