namespace GeekLearning.Authorizations.EntityFrameworkCore.Queries
{
    using GeekLearning.Authorizations.EntityFrameworkCore.Caching;
    using GeekLearning.Authorizations.Model.Client;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class GetScopeRightsQuery<TContext> : IGetScopeRightsQuery where TContext : DbContext
    {
        private readonly TContext context;
        private readonly IAuthorizationsCacheProvider authorizationsCacheProvider;
        private readonly IGetParentGroupsIdQuery getParentGroupsIdQuery;
        private readonly Dictionary<Guid, ConcurrentDictionary<Guid, IEnumerable<Right>>> principalRights;
        private readonly Dictionary<Guid, ConcurrentDictionary<string, IEnumerable<ScopeRights>>> principalScopeRights;
        private readonly Dictionary<Guid, ConcurrentDictionary<string, IEnumerable<ScopeRights>>> principalScopeRightsWithChildren;

        private IDictionary<Guid, Role> roles;
        private IDictionary<Guid, Scope> scopesById;
        private IDictionary<string, Scope> scopesByName;

        public GetScopeRightsQuery(TContext context, IAuthorizationsCacheProvider authorizationsCacheProvider, IGetParentGroupsIdQuery getParentGroupsIdQuery)
        {
            this.context = context;
            this.authorizationsCacheProvider = authorizationsCacheProvider;
            this.getParentGroupsIdQuery = getParentGroupsIdQuery;
            this.principalRights = new Dictionary<Guid, ConcurrentDictionary<Guid, IEnumerable<Right>>>();
            this.principalScopeRights = new Dictionary<Guid, ConcurrentDictionary<string, IEnumerable<ScopeRights>>>();
            this.principalScopeRightsWithChildren = new Dictionary<Guid, ConcurrentDictionary<string, IEnumerable<ScopeRights>>>();
        }

        public async Task<IEnumerable<ScopeRights>> ExecuteAsync(
            string scopeName,
            Guid principalId,
            bool withChildren = false)
        {
            if (!principalRights.ContainsKey(principalId))
            {
                principalRights[principalId] = new ConcurrentDictionary<Guid, IEnumerable<Right>>();
            }

            if (!principalScopeRights.ContainsKey(principalId))
            {
                principalScopeRights[principalId] = new ConcurrentDictionary<string, IEnumerable<ScopeRights>>();
                principalScopeRightsWithChildren[principalId] = new ConcurrentDictionary<string, IEnumerable<ScopeRights>>();
            }

            IEnumerable<ScopeRights> principalScopeRightsCache = null;
            if (this.principalScopeRightsWithChildren[principalId].ContainsKey(scopeName))
            {
                principalScopeRightsCache = this.principalScopeRightsWithChildren[principalId][scopeName];
            }
            else if (!withChildren && this.principalScopeRights[principalId].ContainsKey(scopeName))
            {
                principalScopeRightsCache = this.principalScopeRights[principalId][scopeName];
            }
            else
            {
                var principalIdsLink = new List<Guid>(await this.getParentGroupsIdQuery.ExecuteAsync(principalId.Yield()).ConfigureAwait(false)) { principalId };

                this.roles = await this.authorizationsCacheProvider.GetRolesAsync().ConfigureAwait(false);

                var scopesCache = await this.authorizationsCacheProvider.GetScopesCacheAsync().ConfigureAwait(false);
                this.scopesById = scopesCache.Compute(s => s.Id);
                this.scopesByName = scopesCache.Compute(s => s.Name);

                var principalAuthorizations = await this.context.Authorizations()
                    .Where(a => principalIdsLink.Contains(a.PrincipalId))
                    .Select(a => new { a.ScopeId, a.RoleId, a.Id })
                    .ToListAsync()
                    .ConfigureAwait(false);

                var explicitRightsByScope = principalAuthorizations
                    .GroupBy(a => a.ScopeId)
                    .ToDictionary(
                        ag => ag.Key,
                        ag => ag
                        .SelectMany(a => roles.ContainsKey(a.RoleId) ? roles[a.RoleId].Rights.Select(r => (a.Id, r)) : Enumerable.Empty<(Guid, string)>())
                        .ToArray());

                return await this.GetScopeRightsAsync(this.GetScope(scopeName), principalId, explicitRightsByScope, withChildren).ConfigureAwait(false);                
            }

            return principalScopeRightsCache;
        }

        public void ClearCache()
        {
            this.principalRights.Clear();
            this.principalScopeRights.Clear();
            this.principalScopeRightsWithChildren.Clear();
        }

        private async Task<IEnumerable<ScopeRights>> GetScopeRightsAsync(Scope scope, Guid principalId, IDictionary<Guid, (Guid, string)[]> explicitRightsByScope, bool withChildren = false)
        {
            if (scope == null)
            {
                return Enumerable.Empty<ScopeRights>();
            }

            List<ScopeRights> childrenScopesRights = new List<ScopeRights>();
            if (withChildren && scope.ChildIds?.Any() == true)
            {
                List<Task<IEnumerable<ScopeRights>>> tasks = new List<Task<IEnumerable<ScopeRights>>>();
                foreach (var childScopeId in scope.ChildIds)
                {
                    tasks.Add(Task.Run(() => this.GetScopeRightsAsync(
                        this.GetScope(childScopeId), principalId, explicitRightsByScope, withChildren)));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
                childrenScopesRights.AddRange(tasks.SelectMany(t => t.Result));
            }

            var rightsOnScope = this.DetectRightsOnScope(scope.Id, principalId, this.scopesById, explicitRightsByScope);
            var rightsUnderScope = childrenScopesRights.SelectMany(sr => sr.RightsOnScope.Value.Values).ToList();

            var scopeRights = new List<ScopeRights>(childrenScopesRights)
            {
                new ScopeRights(principalId, scope.Name, rightsOnScope, rightsUnderScope)
            }.Distinct();

            if (withChildren)
            {
                this.principalScopeRightsWithChildren[principalId][scope.Name] = scopeRights;
            }
            else
            {
                this.principalScopeRights[principalId][scope.Name] = scopeRights;
            }

            return scopeRights;
        }

        private IEnumerable<Right> DetectRightsOnScope(Guid scopeId, Guid principalId, IDictionary<Guid, Scope> scopesById, IDictionary<Guid, (Guid, string)[]> explicitRightsByScope)
        {
            if (!scopesById.TryGetValue(scopeId, out Scope scope))
            {
                // TODO: Log Warning!
                return Enumerable.Empty<Right>();
            }

            if (this.principalRights[principalId].ContainsKey(scopeId))
            {
                return this.principalRights[principalId][scopeId];
            }

            List<Right> currentRights = new List<Right>();
            foreach (var parentScopeId in scope.ParentIds ?? Enumerable.Empty<Guid>())
            {
                currentRights.AddRange(
                    this.DetectRightsOnScope(parentScopeId, principalId, scopesById, explicitRightsByScope)
                        .Select(r => new Right(r.PrincipalId, r.ScopeName, r.RightName, false, r.SourceAuthorizations)));
            }

            if (explicitRightsByScope.TryGetValue(scope.Id, out (Guid AuthorizationId, string rightName)[] explicitRightsForScope))
            {
                currentRights.AddRange(
                    explicitRightsForScope.Select(rightData =>
                        new Right(principalId, scope.Name, rightData.rightName, true, rightData.AuthorizationId)));
            }
           
            this.principalRights[principalId][scopeId] = currentRights                
                .Select(x =>
                    new Right(
                        x.PrincipalId,
                        x.ScopeName,
                        x.RightName,
                        x.IsExplicit, x.SourceAuthorizations))
                .Distinct();

            return this.principalRights[principalId][scopeId];
        }

        private Scope GetScope(Guid scopeId)
        {
            if (!this.scopesById.TryGetValue(scopeId, out Scope scope))
            {
                // TODO: Log Warning!
            }

            return scope;
        }

        private Scope GetScope(string scopeName)
        {
            if (!this.scopesByName.TryGetValue(scopeName, out Scope scope))
            {
                // TODO: Log Warning!
            }

            return scope;
        }
    }
}
