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

        public Task<RightsResult> GetRightsAsync(string scopeKey, bool withChildren = false)
        {
            return Task.FromResult(this.rightsResult);
        }

        public Task<bool> HasRightAsync(string rightKey, string scopeKey)
        {
            throw new NotImplementedException();
        }
    }
}
