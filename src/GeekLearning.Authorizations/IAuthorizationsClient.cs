namespace GeekLearning.Authorizations
{
    using System;
    using System.Threading.Tasks;

    public interface IAuthorizationsClient
    {
        Task<Model.PrincipalRights> GetRightsAsync(string scopeName, Guid? principalIdOverride = null, bool withChildren = false);

        Task<bool> HasRightOnScopeAsync(string rightName, string scopeName, Guid? principalIdOverride = null);
    }
}
