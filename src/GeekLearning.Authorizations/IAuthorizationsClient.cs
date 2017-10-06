namespace GeekLearning.Authorizations
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAuthorizationsClient
    {
        Task<Model.PrincipalRights> GetRightsAsync(string scopeName, Guid? principalIdOverride = null, bool withChildren = false);

        Task<bool> HasRightOnScopeAsync(string rightName, string scopeName, Guid? principalIdOverride = null);

        Task<bool> HasExplicitRightOnScopeAsync(string rightName, string scopeName, Guid? principalIdOverride = null);

        Task<IList<Guid>> GetGroupMembersAsync(Guid groupId);

        Task<IList<Guid>> GetGroupParentLinkAsync(Guid principalId);
    }
}
