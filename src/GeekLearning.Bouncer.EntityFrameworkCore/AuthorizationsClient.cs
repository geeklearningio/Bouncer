namespace GeekLearning.Bouncer.EntityFrameworkCore
{
    using GeekLearning.Bouncer.EntityFrameworkCore.Data.Extensions;
    using GeekLearning.Bouncer.EntityFrameworkCore.Queries;
    using GeekLearning.Bouncer.Model.Client;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class AuthorizationsClient<TContext> : IAuthorizationsClient where TContext : DbContext
    {
        private readonly TContext context;
        private readonly IPrincipalIdProvider principalIdProvider;
        private readonly IGetScopeRightsQuery getScopeRightsQuery;
        private readonly IGetParentGroupsIdQuery getParentGroupsIdQuery;

        public AuthorizationsClient(TContext context, IPrincipalIdProvider principalIdProvider, IGetScopeRightsQuery getScopeRightsQuery, IGetParentGroupsIdQuery getParentGroupsIdQuery)
        {
            this.context = context;
            this.principalIdProvider = principalIdProvider;
            this.getScopeRightsQuery = getScopeRightsQuery;
            this.getParentGroupsIdQuery = getParentGroupsIdQuery;
        }

        public async Task<PrincipalRights> GetRightsAsync(string scopeName, Guid? principalIdOverride = null, bool withChildren = false)
        {
            var principalId = principalIdOverride ?? this.principalIdProvider.PrincipalId;
           
            var scopesRights = await this.getScopeRightsQuery.ExecuteAsync(scopeName, principalId, withChildren);

            return new PrincipalRights(principalId, scopeName, scopesRights);
        }

        public async Task<bool> HasRightOnScopeAsync(string rightName, string scopeName, Guid? principalIdOverride = null)
        {
            var principalRights = await this.GetRightsAsync(scopeName, principalIdOverride);
            return principalRights.HasRightOnScope(rightName, scopeName);
        }

        public async Task<bool> HasExplicitRightOnScopeAsync(string rightName, string scopeName, Guid? principalIdOverride = null)
        {
            var principalRights = await this.GetRightsAsync(scopeName, principalIdOverride);
            return principalRights.HasExplicitRightOnScope(rightName, scopeName);
        }

        public async Task<IList<Guid>> GetGroupParentLinkAsync(params Guid[] principalsId)
        {
            return await this.getParentGroupsIdQuery.ExecuteAsync(principalsId);
        }

        public async Task<bool> HasMembershipAsync(params string[] groupNames)
        {
            var userGroupsId = await this.GetGroupParentLinkAsync(this.principalIdProvider.PrincipalId);

            return await this.context
                .Memberships()
                .AnyAsync(m => groupNames.Contains(m.Group.Name) && userGroupsId.Contains(m.GroupId));
        }

        public async Task<IList<string>> DetectMembershipsAsync(IEnumerable<string> groupNames, Guid? principalIdOverride = null)
        {
            var userGroupsId = await this.GetGroupParentLinkAsync(principalIdOverride ?? this.principalIdProvider.PrincipalId);

            return await this.context
                .Memberships()
                .Where(m => groupNames.Contains(m.Group.Name) && userGroupsId.Contains(m.GroupId))
                .Select(m => m.Group.Name)
                .Distinct()
                .ToListAsync();
        }

        public void Reset()
        {
            this.getScopeRightsQuery.ClearCache();
        }
    }
}
