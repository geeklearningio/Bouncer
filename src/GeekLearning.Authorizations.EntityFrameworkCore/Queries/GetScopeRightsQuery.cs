namespace GeekLearning.Authorizations.EntityFrameworkCore.Queries
{
    using GeekLearning.Authorizations.EntityFrameworkCore.Caching;
    using GeekLearning.Authorizations.Model.Client;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class GetScopeRightsQuery<TContext> : IGetScopeRightsQuery where TContext : DbContext
    {
        private readonly TContext context;
        private readonly IAuthorizationsCacheProvider authorizationsCacheProvider;
        private readonly IGetParentGroupsIdQuery getParentGroupsIdQuery;
        private readonly Dictionary<Guid, IDictionary<Guid, IEnumerable<Right>>> principalRights = new Dictionary<Guid, IDictionary<Guid, IEnumerable<Right>>>();
        private readonly Dictionary<Guid, IDictionary<string, IEnumerable<ScopeRights>>> principalScopeRights = new Dictionary<Guid, IDictionary<string, IEnumerable<ScopeRights>>>();
        private readonly Dictionary<Guid, IDictionary<string, IEnumerable<ScopeRights>>> principalScopeRightsWithChildren = new Dictionary<Guid, IDictionary<string, IEnumerable<ScopeRights>>>();

        private IDictionary<Guid, Role> roles;
        private IDictionary<Guid, Scope> scopesById;
        private IDictionary<string, Scope> scopesByName;

        public GetScopeRightsQuery(TContext context, IAuthorizationsCacheProvider authorizationsCacheProvider, IGetParentGroupsIdQuery getParentGroupsIdQuery)
        {
            this.context = context;
            this.authorizationsCacheProvider = authorizationsCacheProvider;
            this.getParentGroupsIdQuery = getParentGroupsIdQuery;
            this.principalRights = new Dictionary<Guid, IDictionary<Guid, IEnumerable<Right>>>();
            this.principalScopeRights = new Dictionary<Guid, IDictionary<string, IEnumerable<ScopeRights>>>();
            this.principalScopeRightsWithChildren = new Dictionary<Guid, IDictionary<string, IEnumerable<ScopeRights>>>();
    }

        public async Task<IEnumerable<ScopeRights>> ExecuteAsync(
            string scopeName,
            Guid principalId,
            bool withChildren = false)
        {
            if (!principalRights.ContainsKey(principalId))
            {
                principalRights[principalId] = new Dictionary<Guid, IEnumerable<Right>>();
            }

            if (!principalScopeRights.ContainsKey(principalId))
            {
                principalScopeRights[principalId] = new Dictionary<string, IEnumerable<ScopeRights>>();
                principalScopeRightsWithChildren[principalId] = new Dictionary<string, IEnumerable<ScopeRights>>();
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
                System.Diagnostics.Stopwatch sw1 = System.Diagnostics.Stopwatch.StartNew();
                var principalIdsLink = new List<Guid>(await this.getParentGroupsIdQuery.ExecuteAsync(principalId)) { principalId };
                sw1.Stop();

                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                this.roles = await this.authorizationsCacheProvider.GetRolesAsync();
                this.scopesById = await this.authorizationsCacheProvider.GetScopesAsync(s => s.Id);
                this.scopesByName = await authorizationsCacheProvider.GetScopesAsync(s => s.Name);
                sw.Stop();

                System.Diagnostics.Stopwatch sw2 = System.Diagnostics.Stopwatch.StartNew();
                var principalAuthorizations = await this.context.Authorizations()
                    .Where(a => principalIdsLink.Contains(a.PrincipalId))
                    .Select(a => new { a.ScopeId, a.RoleId })
                    .ToListAsync();
                var explicitRightsByScope = principalAuthorizations
                    .GroupBy(a => a.ScopeId)
                    .ToDictionary(
                        ag => ag.Key,
                        ag => ag
                        .SelectMany(a => roles.ContainsKey(a.RoleId) ? roles[a.RoleId].Rights : Enumerable.Empty<string>())
                        .ToArray());
                sw2.Stop();

                System.Diagnostics.Stopwatch sw3 = System.Diagnostics.Stopwatch.StartNew();
                var r =  this.ExecuteCore(scopeName, principalId, explicitRightsByScope, withChildren);
                sw3.Stop();

                return r;
            }

            return principalScopeRightsCache;
        }

        private IEnumerable<ScopeRights> ExecuteCore(Guid scopeId, Guid principalId, IDictionary<Guid, string[]> explicitRightsByScope, bool withChildren = false)
        {
            if (!this.scopesById.TryGetValue(scopeId, out Scope scope))
            {
                // TODO: Log Warning!
                return Enumerable.Empty<ScopeRights>();
            }

            return this.GetScopeRights(scope, principalId, explicitRightsByScope, withChildren).Distinct();
        }

        private IEnumerable<ScopeRights> ExecuteCore(string scopeName, Guid principalId, IDictionary<Guid, string[]> explicitRightsByScope, bool withChildren = false)
        {
            if (!this.scopesByName.TryGetValue(scopeName, out Scope scope))
            {
                // TODO: Log Warning!
                return Enumerable.Empty<ScopeRights>();
            }

            return this.GetScopeRights(scope, principalId, explicitRightsByScope, withChildren).Distinct();
        }

        private IEnumerable<ScopeRights> GetScopeRights(Scope scope, Guid principalId, IDictionary<Guid, string[]> explicitRightsByScope, bool withChildren = false)
        {
            List<ScopeRights> childrenScopesRights = new List<ScopeRights>();
            if (withChildren && scope.ChildIds?.Any() == true)
            {
                foreach (var childScopeId in scope.ChildIds)
                {
                    childrenScopesRights.AddRange(this.ExecuteCore(childScopeId, principalId, explicitRightsByScope, withChildren));
                }
            }

            var rightsOnScope = this.DetectRightsOnScope(scope.Id, principalId, this.scopesById, explicitRightsByScope);
            var rightsUnderScope = childrenScopesRights.SelectMany(sr => sr.RightsOnScope.Values).ToList();

            var scopeRights = new List<ScopeRights>(childrenScopesRights)
            {
                new ScopeRights(principalId, scope.Name, rightsOnScope, rightsUnderScope)
            };

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

        private IEnumerable<Right> DetectRightsOnScope(Guid scopeId, Guid principalId, IDictionary<Guid, Scope> scopesById, IDictionary<Guid, string[]> explicitRightsByScope)
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
                currentRights.AddRange(this.DetectRightsOnScope(parentScopeId, principalId, scopesById, explicitRightsByScope));
            }

            if (explicitRightsByScope.TryGetValue(scope.Id, out string[] explicitRightNames))
            {
                currentRights.AddRange(explicitRightNames.Select(rn => new Right(principalId, scope.Name, rn, true)));
            }

            this.principalRights[principalId][scopeId] = currentRights.Distinct();

            return this.principalRights[principalId][scopeId];
        }
    }
}
