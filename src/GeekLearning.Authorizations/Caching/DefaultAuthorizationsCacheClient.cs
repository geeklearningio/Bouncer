namespace GeekLearning.Authorizations.Caching
{
    using Model;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class DefaultAuthorizationsCacheClient : IAuthorizationsCacheClient
    {
        public Task<IEnumerable<ScopeRightsWithParents>> GetRightsAsync(Guid principalId)
        {
            return Task.FromResult<IEnumerable<ScopeRightsWithParents>>(null);
        }

        public Task RemoveRightsAsync(Guid principalId)
        {
            return Task.CompletedTask;
        }

        public Task StoreRightsAsync(Guid principalId, IEnumerable<ScopeRightsWithParents> rights)
        {
            return Task.CompletedTask;
        }
    }
}
