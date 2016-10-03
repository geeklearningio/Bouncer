namespace GeekLearning.Authorizations
{
    using Model;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAuthorizationsProvisioningClient
    {
        Task CreateScopeAsync(string name, string description, params string[] parentKeys);

        Task RenameScopeAsync(string name, string description);

        Task DeleteScopeAsync(string name);

        Task<Scope> GetSecurityScopeAsync(string name);

        Task CreateRightAsync(string name);

        Task RenameRightAsync(string name);

        Task DeleteRightAsync(string name);

        Task CreateRoleAsync(string name, string[] rights);

        Task RenameRoleAsync(string name);

        Task AddRightsToRoleAsync(string name, string[] rights);

        Task RemoveRightsFromRoleAsync(string name, string[] rights);

        Task DeleteRoleAsync(string name);

        Task<IEnumerable<Right>> GetRightsForRoleAsync(string roleName);

        Task AffectRoleToPrincipalAsync(string rolename, Guid principalId, string scopeName);

        Task UnaffectRoleFromPrincipalAsync(string roleName, Guid principalId, string scopeName);
    }
}
