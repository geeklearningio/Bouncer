namespace GeekLearning.Authorizations.EntityFrameworkCore.Caching
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class RolesCache : ICacheableObject
    {
        [JsonProperty("r")]
        public IEnumerable<Role> Roles { get; set; }

        public string CacheKey => GetCacheKey();

        internal IDictionary<Guid, Role> Compute()
        {
            return this.Roles.ToDictionary(r => r.Id, r => r);
        }

        public static string GetCacheKey() => nameof(RolesCache);
    }
}
