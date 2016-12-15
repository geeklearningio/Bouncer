namespace GeekLearning.Authorizations.Caching
{
    using Model;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAuthorizationsCacheClient
    {
        Task StoreRightsAsync(Guid principalId, IEnumerable<ScopeRightsWithParents> rights);

        Task<IEnumerable<ScopeRightsWithParents>> GetRightsAsync(Guid principalId);

        Task RemoveRightsAsync(Guid principalId);
    }
}
