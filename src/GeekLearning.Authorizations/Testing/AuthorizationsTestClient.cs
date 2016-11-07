namespace GeekLearning.Authorizations.Testing
{
    using System;
    using System.Threading.Tasks;
    using Model;

    public class AuthorizationsTestClient : IAuthorizationsClient
    {
        private readonly RightsResult rightsResult;

        private readonly UserRightsProviderService userRightsProvisioningService;

        public AuthorizationsTestClient(UserRightsProviderService userRightsProvisioningService)
        {
            this.userRightsProvisioningService = userRightsProvisioningService;
        }

        public AuthorizationsTestClient(RightsResult rightsResult)
        {
            this.rightsResult = rightsResult;
        }

        public Task<RightsResult> GetRightsAsync(string scopeKey, Guid? principalIdOverride = null, bool withChildren = false)
        {
            return Task.FromResult(this.rightsResult ?? this.userRightsProvisioningService.CurrentRights);
        }

        public async Task<bool> HasRightAsync(string rightKey, string scopeKey, Guid? principalIdOverride = null)
        {
            RightsResult result = await this.GetRightsAsync(scopeKey, principalIdOverride);

            return result.HasRightOnScope(rightKey, scopeKey);
        }
    }
}
