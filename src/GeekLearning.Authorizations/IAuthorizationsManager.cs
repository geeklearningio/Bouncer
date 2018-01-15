namespace GeekLearning.Authorizations
{
    using GeekLearning.Authorizations.Model.Manager;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAuthorizationsManager
    {
        Task CreateScopeAsync(string scopeName, string description, params string[] parents);

        Task DeleteScopeAsync(string scopeName);

        Task LinkScope(string parentScopeName, string childScopeName);

        Task UnlinkScope(string parentScopeName, string childScopeName);

        Task CreateRightAsync(string rightName);

        Task DeleteRightAsync(string rightName);

        Task CreateRoleAsync(string roleName, string[] rightNames);

        Task DeleteRoleAsync(string roleName);

        Task<IGroup> CreateGroupAsync(string groupName, string parentGroupName = null);

        Task DeleteGroupAsync(Guid groupId, bool withChildren = true);

        Task DeleteGroupAsync(string groupName, bool withChildren = true);

        Task AddPrincipalToGroupAsync(Guid principalId, Guid groupId);

        Task AddPrincipalToGroupAsync(Guid principalId, string groupName);

        Task AddPrincipalsToGroupAsync(IEnumerable<Guid> principalIds, string groupName);

        Task AddGroupToGroupAsync(string childGroupName, string parentGroupName);

        Task<IGroup> GetGroupAsync(Guid groupId);

        Task<IGroup> GetGroupAsync(string groupName);

        Task<IList<Guid>> GetGroupMembersAsync(string groupName);

        Task<IList<Guid>> GetGroupMembersAsync(Guid groupId);

        Task<IDictionary<string, IList<Guid>>> GetGroupMembersAsync(params string[] groupNames);

        Task<IList<Guid>> HasMembershipAsync(IEnumerable<Guid> principalIds, params string[] groupNames);

        Task RemovePrincipalFromGroupAsync(Guid principalId, Guid groupId);

        Task RemovePrincipalFromGroupAsync(Guid principalId, string groupName);

        Task RemovePrincipalsFromGroupAsync(IEnumerable<Guid> principalIds, string groupName);

        Task RemoveAllPrincipalsFromGroupAsync(string groupName);

        Task AffectRoleToPrincipalOnScopeAsync(string roleName, Guid principalId, string scopeName);

        Task AffectRoleToGroupOnScopeAsync(string roleName, string groupName, string scopeName);

        Task UnaffectRoleFromPrincipalOnScopeAsync(string roleName, Guid principalId, string scopeName);

        Task UnaffectRoleFromGroupOnScopeAsync(string roleName, string groupName, string scopeName);

        Task UnaffectRolesFromGroupAsync(string groupName);
    }
}
