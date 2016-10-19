namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using System;
    using System.Threading.Tasks;
    using Model;

    public class AuthorizationsTestClient : IAuthorizationsClient
    {
        private RightsResult rightsResult;

        public AuthorizationsTestClient(RightsResult rightsResult)
        {
            this.rightsResult = rightsResult;
        }

        public Task<RightsResult> GetRightsAsync(string scopeKey, Guid? principalIdOverride = null, bool withChildren = false)
        {
            return Task.FromResult(this.rightsResult);
        }

        public async Task<bool> HasRightAsync(string rightKey, string scopeKey, Guid? principalIdOverride = null)
        {
            RightsResult result = await this.GetRightsAsync(scopeKey, principalIdOverride);

            return result.HasRightOnScope(rightKey, scopeKey);
        }
    }
}
