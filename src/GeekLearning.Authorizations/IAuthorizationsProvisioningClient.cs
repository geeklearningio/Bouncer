namespace GeekLearning.Authorizations
{
    using Data;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAuthorizationsProvisioningClient
    {
        Task CreateScopeAsync(string name, string description, params string[] parents);

        Task DeleteScopeAsync(string name);

        Task CreateRightAsync(string name);

        Task DeleteRightAsync(string name);

        Task CreateRoleAsync(string name, string[] rights);
        
        Task DeleteRoleAsync(string name);

        Task AffectRoleToPrincipalOnScopeAsync(string roleName, Guid principalId, string scopeName);

        Task UnaffectRoleFromPrincipalOnScopeAsync(string roleName, Guid principalId, string scopeName);
    }
}
