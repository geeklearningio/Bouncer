namespace GeekLearning.Authorizations.Caching
{
    using Model;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAuthorizationsCacheClient
    {
        Task StoreRightsAsync(Guid principalId, IEnumerable<ScopeRights> rights);

        Task<IEnumerable<ScopeRights>> GetRightsAsync(Guid principalId);

        Task RemoveRightsAsync(Guid principalId);
    }
}
