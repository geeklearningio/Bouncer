namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using Caching;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Model;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    public class AuthorizationsClient
        <TContext> : IAuthorizationsClient where TContext : DbContext
    {
        private readonly TContext context;
        private readonly IPrincipalIdProvider principalIdProvider;
        private readonly IAuthorizationsCacheClient cacheClient;
        private readonly Dictionary<Guid, IEnumerable<ScopeRights>> scopeRightsScopedCache = new Dictionary<Guid, IEnumerable<ScopeRights>>();

        public AuthorizationsClient(TContext context, IPrincipalIdProvider principalIdProvider, IAuthorizationsCacheClient cacheClient)
        {
            this.context = context;
            this.principalIdProvider = principalIdProvider;
            this.cacheClient = cacheClient;
        }

        public async Task<RightsResult> GetRightsAsync(string scopeKey, Guid? principalIdOverride = null, bool withChildren = false)
        {
            var principalId = principalIdOverride ?? this.principalIdProvider.PrincipalId;

            if (scopeRightsScopedCache.ContainsKey(principalId))
            {
                return this.scopeRightsScopedCache[principalId].GetResultForScopeName(scopeKey, withChildren);
            }

            var rightsFromCache = await this.cacheClient.GetRightsAsync(principalId);
            if (rightsFromCache != null)
            {
                this.scopeRightsScopedCache.Add(principalId, rightsFromCache);
                return rightsFromCache.GetResultForScopeName(scopeKey, withChildren);
            }

            using (RelationalDataReader dataReader = await this.context.Database.ExecuteSqlCommandExtAsync(
                $"select * from Authorizations.PrincipalScopeRight where PrincipalId = @principalId",
                parameters: new object[]
                {
                    new SqlParameter("@principalId", principalId)
                }))
            {
                var rights = await dataReader.FromFlatResultToRightsResultAsync();
                await this.cacheClient.StoreRightsAsync(principalId, rights);
                this.scopeRightsScopedCache.Add(principalId, rightsFromCache);
                return rights.GetResultForScopeName(scopeKey, withChildren);
            }
        }

        public async Task<bool> HasRightAsync(string rightKey, string scopeKey, Guid? principalIdOverride = null)
        {
            RightsResult result = await this.GetRightsAsync(scopeKey, principalIdOverride);
            return result.HasRightOnScope(rightKey, scopeKey);
        }
    }
}
