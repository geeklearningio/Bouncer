namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Threading.Tasks;
    using System.Data;
    using Model;
    using System.Data.Common;
    using System.Data.SqlClient;
    using GeekLearning.Authorizations.Projections;
    using Microsoft.EntityFrameworkCore.Storage;

    public class AuthorizationsClient
        <TContext> : IAuthorizationsClient where TContext : DbContext
    {
        private readonly TContext context;

        private readonly IPrincipalIdProvider principalIdProvider;

        public AuthorizationsClient(TContext context, IPrincipalIdProvider principalIdProvider)
        {
            this.context = context;
            this.principalIdProvider = principalIdProvider;
        }

        public async Task<RightsResult> GetRightsAsync(string scopeKey, Guid? principalIdOverride = null, bool withChildren = false)
        {
            var function = withChildren ? "Authorizations.GetRightsForScopeAndChildren" : "Authorizations.GetRightsForScope";
            
            using (RelationalDataReader dataReader = await this.context.Database.ExecuteSqlCommandExtAsync(
                                                                                    $"select * from {function}(@scopeName,@principalId)",
                                                                                    parameters: new object[]
                                                                                    {
                                                                                        new SqlParameter("@scopeName", scopeKey),
                                                                                        new SqlParameter("@principalId", principalIdOverride ?? this.principalIdProvider.PrincipalId)
                                                                                    }))
            {
                return await dataReader.ToRightsResultAsync();
            }
        }

        public async Task<bool> HasRightAsync(string rightKey, string scopeKey, Guid? principalIdOverride = null)
        {
            RightsResult result = await this.GetRightsAsync(scopeKey, principalIdOverride);

            return result.HasRightOnScope(rightKey, scopeKey);
        }
    }
}
