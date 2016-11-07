namespace GeekLearning.Authorizations.Testing
{
    using System;
    using System.Threading.Tasks;
    using Model;
    using System.Collections.Generic;

    public class AuthorizationsProvisioningTestClient : IAuthorizationsProvisioningClient
    {
        private Dictionary<string, string[]> roleRights = new Dictionary<string, string[]>();

        private Dictionary<Guid, Dictionary<string, IList<string>>> principalScopeRoles = new Dictionary<Guid, Dictionary<string, IList<string>>>();

        public Task AffectRoleToPrincipalOnScopeAsync(string roleKey, Guid principalId, string scopeKey)
        {
            if (this.principalScopeRoles[principalId][scopeKey] == null)
            {
                this.principalScopeRoles[principalId][scopeKey] = new List<string>();
            }

            if (!this.principalScopeRoles[principalId][scopeKey].Contains(roleKey))
            {
                this.principalScopeRoles[principalId][scopeKey].Add(roleKey);
            }

            return Task.CompletedTask;
        }

        public Task CreateRightAsync(string rightKey)
        {
            return Task.CompletedTask;
        }

        public Task CreateRoleAsync(string roleKey, string[] rights)
        {
            this.roleRights[roleKey] = rights;

            return Task.CompletedTask;
        }

        public Task CreateScopeAsync(string scopeKey, string description, params string[] parents)
        {
            return Task.CompletedTask;
        }

        public Task DeleteRightAsync(string rightKey)
        {
            return Task.CompletedTask;
        }

        public Task DeleteRoleAsync(string roleKey)
        {
            this.roleRights.Remove(roleKey);

            return Task.CompletedTask;
        }

        public Task DeleteScopeAsync(string scopeKey)
        {
            foreach (var principalAuthorizations in this.principalScopeRoles)
            {
                if (principalAuthorizations.Value.ContainsKey(scopeKey))
                {
                    principalAuthorizations.Value.Remove(scopeKey);
                }
            }

            return Task.CompletedTask;
        }

        public Task UnaffectRoleFromPrincipalOnScopeAsync(string roleKey, Guid principalId, string scopeKey)
        {
            if (this.principalScopeRoles.ContainsKey(principalId) && this.principalScopeRoles[principalId].ContainsKey(scopeKey))
            {
                this.principalScopeRoles[principalId][scopeKey].Remove(roleKey);
            }

            return Task.CompletedTask;
        }
    }
}
