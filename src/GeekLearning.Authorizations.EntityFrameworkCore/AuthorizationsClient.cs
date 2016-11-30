namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Model;
    using Projections;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    public class AuthorizationsClient
        <TContext> : IAuthorizationsClient where TContext : DbContext
    {
        private readonly TContext context;
        private readonly IPrincipalIdProvider principalIdProvider;
        private readonly Dictionary<Guid, List<ScopeRightsWithParents>> flatResultsCache = new Dictionary<Guid, List<ScopeRightsWithParents>>();

        public AuthorizationsClient(TContext context, IPrincipalIdProvider principalIdProvider)
        {
            this.context = context;
            this.principalIdProvider = principalIdProvider;
        }

        public async Task<RightsResult> GetRightsAsync(string scopeKey, Guid? principalIdOverride = null, bool withChildren = false)
        {
            var principalId = principalIdOverride ?? this.principalIdProvider.PrincipalId;

            if (flatResultsCache.ContainsKey(principalId))
            {
                return flatResultsCache[principalId].GetResultForScopeName(scopeKey, withChildren);
            }

            using (RelationalDataReader dataReader = await this.context.Database.ExecuteSqlCommandExtAsync(
                $"select * from Authorizations.PrincipalScopeRight where PrincipalId = @principalId",
                parameters: new object[]
                {
                    new SqlParameter("@principalId", principalId)
                }))
            {
                var rights = await dataReader.FromFlatResultToRightsResultAsync();
                flatResultsCache.Add(principalId, rights);
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
