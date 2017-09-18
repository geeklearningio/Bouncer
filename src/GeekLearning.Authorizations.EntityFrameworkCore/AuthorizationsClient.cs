namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using Authorizations.Model;
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

            Dictionary<Guid, ParsedScope> parsedScopes;
            if (!this.parsedScopesPerPrincipal.TryGetValue(principalId, out parsedScopes))
            {
                var roles = await this.authorizationsCacheProvider.GetRolesAsync();
                var scopes = await this.authorizationsCacheProvider.GetScopesAsync();

                var principalRightsPerScope = (await this.context.Authorizations()
                    .Where(a => a.PrincipalId == principalId)
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
    }
}
