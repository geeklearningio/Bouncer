namespace GeekLearning.Bouncer.EntityFrameworkCore.Queries
{
    using Bouncer.Model.Client;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IGetScopeRightsQuery
    {
        Task<IEnumerable<ScopeRights>> ExecuteAsync(string scopeName, Guid principalId, bool withChildren = false);

        void ClearCache();
    }
}