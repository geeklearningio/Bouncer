namespace GeekLearning.Authorizations
{
    using System;
    using System.Threading.Tasks;

    public interface IAuthorizationsProvisioningClient
    {
        Task CreateScopeAsync(string scopeKey, string description, params string[] parents);

        Task DeleteScopeAsync(string scopeKey);

        Task CreateRightAsync(string rightKey);

        Task DeleteRightAsync(string rightKey);

        Task CreateRoleAsync(string roleKey, string[] rights);
        
        Task DeleteRoleAsync(string roleKey);

        Task AffectRoleToPrincipalOnScopeAsync(string roleKey, Guid principalId, string scopeKey);

        Task UnaffectRoleFromPrincipalOnScopeAsync(string roleKey, Guid principalId, string scopeKey);
    }
}
