namespace GeekLearning.Authorizations.Caching
{
    using Model;
    using System;
    using System.Threading.Tasks;

    public interface IAuthorizationsCacheClient
    {
        Task StoreRightsAsync(Guid principalId, RightsResult rightsResult);

        Task<RightsResult> GetRightsAsync(Guid principalId);

        Task RemoveRightsAsync(Guid principalId);
    }
}
