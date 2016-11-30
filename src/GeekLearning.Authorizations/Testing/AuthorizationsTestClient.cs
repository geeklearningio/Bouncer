namespace GeekLearning.Authorizations.Testing
{
    using Model;
    using System;
    using System.Threading.Tasks;

    public class AuthorizationsTestClient : IAuthorizationsClient
    {
        private readonly UserRightsProviderService userRightsProvisioningService;

        public AuthorizationsTestClient(UserRightsProviderService userRightsProvisioningService)
        {
            this.userRightsProvisioningService = userRightsProvisioningService;
        }

        public AuthorizationsTestClient(UserRightsProviderService userRightsProvisioningService, RightsResult rightsResult)
            : this(userRightsProvisioningService)
        {
            this.userRightsProvisioningService.CurrentRights = rightsResult;
        }

        public Task<RightsResult> GetRightsAsync(string scopeKey, Guid? principalIdOverride = null, bool withChildren = false)
        {
            return Task.FromResult(this.userRightsProvisioningService.CurrentRights);
        }

        public async Task<bool> HasRightAsync(string rightKey, string scopeKey, Guid? principalIdOverride = null)
        {
            RightsResult result = await this.GetRightsAsync(scopeKey, principalIdOverride);

            return result.HasRightOnScope(rightKey, scopeKey);
        }
    }
}
