namespace GeekLearning.Authorizations.Tests
{
    using GeekLearning.Authorizations.Events;
    using GeekLearning.Authorizations.Events.Model;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class AuthorizationsTestImpactClient : IAuthorizationsImpactClient
    {
        private readonly IAuthorizationsClient authorizationsClient;

        public AuthorizationsTestImpactClient(IAuthorizationsClient authorizationsClient)
        {
            this.authorizationsClient = authorizationsClient;
        }

        public IDictionary<Guid, string> UserDenormalizedRights { get; } = new Dictionary<Guid, string>();

        public async Task StoreAuthorizationsImpactAsync(AuthorizationsImpact authorizationsImpact)
        {
            foreach (var impactedUserId in authorizationsImpact.UserIds)
            {
                foreach (var impactedScopeName in authorizationsImpact.ScopeNames)
                {
                    var rightsOnScopeWithChildren = await this.authorizationsClient.GetRightsAsync(impactedScopeName, withChildren: true);

                    this.UserDenormalizedRights[impactedUserId] = JsonConvert.SerializeObject(rightsOnScopeWithChildren);
                }
            }
        }
    }
}
