namespace GeekLearning.Authorizations
{
    using Model;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAuthorizationsClient
    {
        Task<RightsResult> GetRightsAsync(string scopeKey, Guid? principalIdOverride = null, bool withChildren = false);

        Task<bool> HasRightAsync(string rightKey, string scopeKey, Guid? principalIdOverride = null);
    }
}
