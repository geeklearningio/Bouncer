namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using Authorizations.Model.Client;
    using GeekLearning.Authorizations.EntityFrameworkCore.Exceptions;
    using GeekLearning.Authorizations.EntityFrameworkCore.Queries;
    using Microsoft.EntityFrameworkCore;
    using Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class AuthorizationsClient<TContext> : IAuthorizationsClient where TContext : DbContext
    {
        private readonly TContext context;
        private readonly IPrincipalIdProvider principalIdProvider;
        private readonly Caching.IAuthorizationsCacheProvider authorizationsCacheProvider;
        private readonly Dictionary<Guid, Dictionary<string, ScopeRights>> principalsScopesRights = new Dictionary<Guid, Dictionary<string, ScopeRights>>();

        public AuthorizationsClient(TContext context, IPrincipalIdProvider principalIdProvider, Caching.IAuthorizationsCacheProvider authorizationsCacheProvider)
        {
            this.context = context;
            this.principalIdProvider = principalIdProvider;
            this.authorizationsCacheProvider = authorizationsCacheProvider;
        }

        public async Task<PrincipalRights> GetRightsAsync(string scopeName, Guid? principalIdOverride = null, bool withChildren = false)
        {
            var principalId = principalIdOverride ?? this.principalIdProvider.PrincipalId;

            var roles = await this.authorizationsCacheProvider.GetRolesAsync();
            var scopes = await this.authorizationsCacheProvider.GetScopesAsync(s => s.Id);
            var scopesByName = await this.authorizationsCacheProvider.GetScopesAsync(s => s.Name);

            var principalIdsLink = await this.GetGroupParentLinkAsync(principalId);
            principalIdsLink.Add(principalId);

            var principalAuthorizations = await this.context.Authorizations()
                .Where(a => principalIdsLink.Contains(a.PrincipalId))
                .Select(a => new { a.ScopeId, a.RoleId })
                .ToListAsync();
            var principalRightsByScope = principalAuthorizations
                .GroupBy(a => a.ScopeId)
                .ToDictionary(
                    ag => ag.Key,
                    ag => ag.SelectMany(a => roles.ContainsKey(a.RoleId) ? roles[a.RoleId].Rights : Enumerable.Empty<string>()).ToArray());           

            var rootScopeRights = new GetScopeRightsQuery(scopes, scopesByName, principalRightsByScope)
                .Execute(scopeName, principalId, withChildren);

            return new PrincipalRights(principalId, scopeName, rootScopeRights);
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
            List<Guid> groupIds = new List<Guid>();
            var groupParentsId = await this.context.Memberships()
                .Where(m => principalsId.Contains(m.PrincipalId))
                .Select(m => m.GroupId)
                .ToListAsync();
            groupIds.AddRange(groupParentsId);

            if (groupParentsId.Count > 0)
            {
                groupIds.AddRange(await GetGroupParentLinkAsync(groupParentsId.ToArray()));
            }

            return groupIds;
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
    }
}
