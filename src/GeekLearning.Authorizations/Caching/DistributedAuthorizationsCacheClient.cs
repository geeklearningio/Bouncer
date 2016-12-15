namespace GeekLearning.Authorizations.Caching
{
    using Microsoft.Extensions.Caching.Distributed;
    using Model;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public class DistributedAuthorizationsCacheClient : IAuthorizationsCacheClient
    {
        private readonly IDistributedCache cache;

        public DistributedAuthorizationsCacheClient(IDistributedCache cache)
        {
            this.cache = cache;
        }

        public async Task<IEnumerable<ScopeRightsWithParents>> GetRightsAsync(Guid principalId)
        {
            var fromCache = await this.cache.GetAsync(CacheKeyUtilities.GetCacheKey(principalId));
            if (fromCache == null)
            {
                return null;
            }
            var str = Encoding.UTF8.GetString(fromCache);
            return JsonConvert.DeserializeObject<IEnumerable<ScopeRightsWithParents>>(str);
        }

        public async Task RemoveRightsAsync(Guid principalId)
        {
            await this.cache.RemoveAsync(CacheKeyUtilities.GetCacheKey(principalId));
        }

        public async Task StoreRightsAsync(Guid principalId, IEnumerable<ScopeRightsWithParents> rights)
        {
            var toStore = JsonConvert.SerializeObject(rights);
            await this.cache.SetAsync(CacheKeyUtilities.GetCacheKey(principalId), Encoding.UTF8.GetBytes(toStore));
        }
    }
}
