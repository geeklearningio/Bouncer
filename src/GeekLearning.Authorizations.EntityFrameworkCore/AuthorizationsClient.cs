namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using Authorizations.Model;
    using GeekLearning.Authorizations.EntityFrameworkCore.Exceptions;
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
        private readonly Dictionary<Guid, Dictionary<Guid, ParsedScope>> parsedScopesPerPrincipal = new Dictionary<Guid, Dictionary<Guid, ParsedScope>>();

        public AuthorizationsClient(TContext context, IPrincipalIdProvider principalIdProvider, Caching.IAuthorizationsCacheProvider authorizationsCacheProvider)
        {
            this.context = context;
            this.principalIdProvider = principalIdProvider;
            this.authorizationsCacheProvider = authorizationsCacheProvider;
        }

        public async Task<PrincipalRights> GetRightsAsync(string scopeName, Guid? principalIdOverride = null, bool withChildren = false)
        {
            var principalId = principalIdOverride ?? this.principalIdProvider.PrincipalId;

            if (!this.parsedScopesPerPrincipal.TryGetValue(principalId, out Dictionary<Guid, ParsedScope> parsedScopes))
            {
                var roles = await this.authorizationsCacheProvider.GetRolesAsync();
                var scopes = await this.authorizationsCacheProvider.GetScopesAsync();

                var principalIdsLink = await this.GetGroupParentLinkAsync(principalId);
                principalIdsLink.Add(principalId);

                var principalRightsPerScope = (await this.context.Authorizations()
                    .Where(a => principalIdsLink.Contains(a.PrincipalId))
                    .Select(a => new { a.ScopeId, a.RoleId })
                    .ToListAsync())
                    .GroupBy(a => a.ScopeId)
                    .ToDictionary(
                        ag => ag.Key,
                        ag => ag.SelectMany(a => roles.ContainsKey(a.RoleId) ? roles[a.RoleId].Rights : Enumerable.Empty<string>()).ToArray());
               
                var rootScopes = scopes
                    .Where(s => s.Value.ParentIds == null || !s.Value.ParentIds.Any())
                    .Select(s => s.Value)
                    .ToList();

                if (rootScopes.Count == 0)
                {
                    throw new RootScopeNotFoundException();
                }

                parsedScopes = new Dictionary<Guid, ParsedScope>();
                foreach (var rootScope in rootScopes)
                {
                    ParsedScope.Parse(rootScope.Id, scopes, principalRightsPerScope, parsedScopes);
                }

                this.parsedScopesPerPrincipal.Add(principalId, parsedScopes);
            }

            var askedParsedScope = parsedScopes.Values.FirstOrDefault(s => s.Scope.Name == scopeName);
            if (askedParsedScope == null)
            {
                return new PrincipalRights(principalId, scopeName, Enumerable.Empty<ScopeRights>(), scopeNotFound: true);
            }

            return askedParsedScope.ToPrincipalRights(principalId);
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

        public async Task<IList<Guid>> GetGroupMembersAsync(Guid groupId)
        {
            List<Guid> principalIds = new List<Guid>();
            var groupMembers = await this.context.Memberships()
                .Where(m => m.GroupId == groupId)
                .ToListAsync();
            principalIds.AddRange(groupMembers.Select(gm => gm.PrincipalId));
            foreach (var groupMember in groupMembers)
            {
                principalIds.AddRange(await GetGroupMembersAsync(groupMember.PrincipalId));
            }

            return principalIds;
        }

        public async Task<IList<Guid>> GetGroupMembersAsync(string groupName)
        {
            return await this.GetGroupMembersAsync(
                await this.context.Groups().Where(g => g.Name == groupName).Select(g => g.Id).FirstOrDefaultAsync());
        }

        public async Task<IDictionary<string, IList<Guid>>> GetGroupMembersAsync(params string[] groupNames)
        {
            Dictionary<string, IList<Guid>> groupMembers = new Dictionary<string, IList<Guid>>();
            // To be improved with a single query
            foreach (var groupName in groupNames)
            {
                groupMembers[groupName] = await this.GetGroupMembersAsync(groupName);
            }

            return groupMembers;
        }
        public async Task<IList<Guid>> GetGroupParentLinkAsync(Guid principalId)
        {
            List<Guid> groupIds = new List<Guid>();
            var groupParents = await this.context.Memberships()
                .Where(m => m.PrincipalId == principalId)
                .ToListAsync();
            groupIds.AddRange(groupParents.Select(gm => gm.GroupId));
            foreach (var groupParent in groupParents)
            {
                groupIds.AddRange(await GetGroupParentLinkAsync(groupParent.GroupId));
            }

            return groupIds;
        }

        public async Task<bool> HasMembershipAsync(params string[] groupNames)
        {
            return await this.context
                .Memberships()
                .Join(groupNames, m => m.Group.Name, groupName => groupName, (m, groupName) => m)
                .AnyAsync(m => m.PrincipalId == this.principalIdProvider.PrincipalId);
        }

        public async Task<IList<Guid>> HasMembershipAsync(IEnumerable<Guid> principalIds, params string[] groupNames)
        {
            return await this.context
                .Memberships()
                .Join(principalIds, m => m.PrincipalId, pId => pId, (m, pId) => m)
                .Join(groupNames, m => m.Group.Name, groupName => groupName, (m, groupName) => m)                
                .Select(m => m.PrincipalId)
                .ToListAsync();
        }

        public async Task<IList<string>> DetectMembershipsAsync(IEnumerable<string> groupNames)
        {
            return await this.context
                .Memberships()
                .Join(groupNames, m => m.Group.Name, groupName => groupName, (m, groupName) => groupName)
                .ToListAsync();
        }
    }
}
