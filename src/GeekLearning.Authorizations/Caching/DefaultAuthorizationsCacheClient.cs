namespace GeekLearning.Authorizations.Caching
{
    using Model;
    using Newtonsoft.Json;
    using System;
    using System.Threading.Tasks;

    public class DefaultAuthorizationsCacheClient : IAuthorizationsCacheClient
    {
        public Task<RightsResult> GetRightsAsync(Guid principalId)
        {
            return Task.FromResult<RightsResult>(null);
        }

        public Task RemoveRightsAsync(Guid principalId)
        {
            return Task.CompletedTask;
        }

        public Task StoreRightsAsync(Guid principalId, RightsResult rightsResult)
        {
            var toStore = JsonConvert.SerializeObject(rightsResult);
            return Task.FromResult(toStore);
        }
    }
}
