namespace GeekLearning.Authorizations
{
    using System;
    using System.Threading.Tasks;

    public interface IAuthorizationsProvisioningClient
    {
        Task CreateScopeAsync(string scopeName, string description, params string[] parents);

        Task DeleteScopeAsync(string scopeName);

        Task CreateRightAsync(string rightName);

        Task DeleteRightAsync(string rightName);

        Task CreateRoleAsync(string roleName, string[] rightNames);
        
        Task DeleteRoleAsync(string roleName);

        Task AffectRoleToPrincipalOnScopeAsync(string roleName, Guid principalId, string scopeName);

        Task UnaffectRoleFromPrincipalOnScopeAsync(string roleName, Guid principalId, string scopeName);
    }
}
