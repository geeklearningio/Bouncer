namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using Caching;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Model;
    using System;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    public class AuthorizationsClient
        <TContext> : IAuthorizationsClient where TContext : DbContext
    {
        private readonly TContext context;
        private readonly IPrincipalIdProvider principalIdProvider;
        private readonly IAuthorizationsCacheClient cacheClient;

        public AuthorizationsClient(TContext context, IPrincipalIdProvider principalIdProvider, IAuthorizationsCacheClient cacheClient)
        {
            this.context = context;
            this.principalIdProvider = principalIdProvider;
            this.cacheClient = cacheClient;
        }

        public async Task<RightsResult> GetRightsAsync(string scopeKey, Guid? principalIdOverride = null, bool withChildren = false)
        {
            var principalId = principalIdOverride ?? this.principalIdProvider.PrincipalId;

            var fromCache = await this.cacheClient.GetRightsAsync(principalId);
            if (fromCache != null)
            {
                return fromCache.GetResultForScopeName(scopeKey, withChildren);
            }

            using (RelationalDataReader dataReader = await this.context.Database.ExecuteSqlCommandExtAsync(
                $"select * from Authorizations.PrincipalScopeRight where PrincipalId = @principalId",
                parameters: new object[]
                {
                    new SqlParameter("@principalId", principalId)
                }))
            {
                var rights = await dataReader.FromFlatResultToRightsResultAsync();
                var rightsResult = new RightsResult(rights);
                await this.cacheClient.StoreRightsAsync(principalId, rightsResult);

                return rightsResult.GetResultForScopeName(scopeKey, withChildren);
            }
        }

        public async Task<bool> HasRightAsync(string rightKey, string scopeKey, Guid? principalIdOverride = null)
        {
            RightsResult result = await this.GetRightsAsync(scopeKey, principalIdOverride);
            return result.HasRightOnScope(rightKey, scopeKey);
        }
    }
}
