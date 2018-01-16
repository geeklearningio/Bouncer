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
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, IEnumerable<Right>>> principalRights;
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, IEnumerable<ScopeRights>>> principalScopeRights;
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, IEnumerable<ScopeRights>>> principalScopeRightsWithChildren;

        private IDictionary<Guid, Role> roles;
        private IDictionary<Guid, Scope> scopesById;
        private IDictionary<string, Scope> scopesByName;

        public GetScopeRightsQuery(TContext context, IAuthorizationsCacheProvider authorizationsCacheProvider, IGetParentGroupsIdQuery getParentGroupsIdQuery)
        {
            this.context = context;
            this.authorizationsCacheProvider = authorizationsCacheProvider;
            this.getParentGroupsIdQuery = getParentGroupsIdQuery;
            this.principalRights = new ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, IEnumerable<Right>>>();
            this.principalScopeRights = new ConcurrentDictionary<Guid, ConcurrentDictionary<string, IEnumerable<ScopeRights>>>();
            this.principalScopeRightsWithChildren = new ConcurrentDictionary<Guid, ConcurrentDictionary<string, IEnumerable<ScopeRights>>>();
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
                var r = await this.ExecuteCoreAsync(scopeName, principalId, explicitRightsByScope, withChildren, true);
                sw3.Stop();

                return r;
            }

            return principalScopeRightsCache;
        }

        private async Task<IEnumerable<ScopeRights>> ExecuteCoreAsync(Guid scopeId, Guid principalId, IDictionary<Guid, string[]> explicitRightsByScope, bool withChildren = false, bool rootChildLevel = false)
        {
            if (!this.scopesById.TryGetValue(scopeId, out Scope scope))
            {
                // TODO: Log Warning!
                return Enumerable.Empty<ScopeRights>();
            }

            return await this.GetScopeRightsAsync(scope, principalId, explicitRightsByScope, withChildren, rootChildLevel);            
        }

        private async Task<IEnumerable<ScopeRights>> ExecuteCoreAsync(string scopeName, Guid principalId, IDictionary<Guid, string[]> explicitRightsByScope, bool withChildren = false, bool rootChildLevel = false)
        {
            if (!this.scopesByName.TryGetValue(scopeName, out Scope scope))
            {
                // TODO: Log Warning!
                return Enumerable.Empty<ScopeRights>();
            }

            return await this.GetScopeRightsAsync(scope, principalId, explicitRightsByScope, withChildren, rootChildLevel);            
        }

        private async Task<IEnumerable<ScopeRights>> GetScopeRightsAsync(Scope scope, Guid principalId, IDictionary<Guid, string[]> explicitRightsByScope, bool withChildren = false, bool rootChildLevel = false)
        {
            List<ScopeRights> childrenScopesRights = new List<ScopeRights>();
            if (withChildren && scope.ChildIds?.Any() == true)
            {
                List<Task<IEnumerable<ScopeRights>>> tasks = new List<Task<IEnumerable<ScopeRights>>>();
                foreach (var childScopeId in scope.ChildIds)
                {
                    if (rootChildLevel)
                    {
                        tasks.Add(Task.Run(async () => await this.ExecuteCoreAsync(childScopeId, principalId, explicitRightsByScope, withChildren)));
                    }
                    else
                    {
                        childrenScopesRights.AddRange(await this.ExecuteCoreAsync(childScopeId, principalId, explicitRightsByScope, withChildren));
                    }
                }

                if (tasks.Count > 0)
                {
                    await Task.WhenAll(tasks.ToArray());
                    childrenScopesRights.AddRange(tasks.SelectMany(t => t.Result));
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

            return scopeRights.Distinct();
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
