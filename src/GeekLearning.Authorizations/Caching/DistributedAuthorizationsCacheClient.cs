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

        public async Task<IEnumerable<ScopeRights>> GetRightsAsync(Guid principalId)
        {
            var fromCache = await this.cache.GetAsync(CacheKeyUtilities.GetCacheKey(principalId));
            if (fromCache == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<IEnumerable<ScopeRights>>(Encoding.UTF8.GetString(fromCache));
        }

        public async Task RemoveRightsAsync(Guid principalId)
        {
            await this.cache.RemoveAsync(CacheKeyUtilities.GetCacheKey(principalId));
        }

        public async Task StoreRightsAsync(Guid principalId, IEnumerable<ScopeRights> rights)
        {
            var toStore = JsonConvert.SerializeObject(rights);
            await this.cache.SetAsync(CacheKeyUtilities.GetCacheKey(principalId), Encoding.UTF8.GetBytes(toStore));
        }
    }
}
