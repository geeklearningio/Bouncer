namespace GeekLearning.Authorizations
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAuthorizationsProvisioningClient
    {
        Task CreateScopeAsync(string scopeName, string description, params string[] parents);

        Task DeleteScopeAsync(string scopeName);

        Task CreateRightAsync(string rightName);

        Task DeleteRightAsync(string rightName);

        Task CreateRoleAsync(string roleName, string[] rightNames);
        
        Task DeleteRoleAsync(string roleName);

        Task CreateGroupAsync(string groupName);

        Task DeleteGroupAsync(string groupName);

        Task AddPrincipalToGroup(Guid principalId, string groupName);

        Task AddPrincipalsToGroup(IEnumerable<Guid> principalIds, string groupName);

        Task AffectRoleToPrincipalOnScopeAsync(string roleName, Guid principalId, string scopeName);

        Task UnaffectRoleFromPrincipalOnScopeAsync(string roleName, Guid principalId, string scopeName);
    }
}
