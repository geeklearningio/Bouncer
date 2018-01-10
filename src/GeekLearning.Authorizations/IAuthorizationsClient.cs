namespace GeekLearning.Authorizations
{
    using GeekLearning.Authorizations.Model.Client;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAuthorizationsClient
    {
        Task<PrincipalRights> GetRightsAsync(string scopeName, Guid? principalIdOverride = null, bool withChildren = false);

        Task<bool> HasRightOnScopeAsync(string rightName, string scopeName, Guid? principalIdOverride = null);

        Task<bool> HasExplicitRightOnScopeAsync(string rightName, string scopeName, Guid? principalIdOverride = null);
        
        Task<IList<Guid>> GetGroupParentLinkAsync(params Guid[] principalsId);

        Task<bool> HasMembershipAsync(params string[] groupNames);

        Task<IList<string>> DetectMembershipsAsync(IEnumerable<string> groupNames, Guid? principalIdOverride = null);

        void Reset();
    }
}
