﻿namespace GeekLearning.Authorizations.Testing
{
    using Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class AuthorizationsProvisioningTestClient : IAuthorizationsProvisioningClient
    {
        private readonly UserRightsProviderService userRightsProvisioningService;

        private Dictionary<string, string[]> roleRights = new Dictionary<string, string[]>();

        public AuthorizationsProvisioningTestClient(UserRightsProviderService userRightsProvisioningService)
        {
            this.userRightsProvisioningService = userRightsProvisioningService;
        }

        public Task AffectRoleToPrincipalOnScopeAsync(string roleKey, Guid principalId, string scopeKey)
        {
            var scopeRights = this.userRightsProvisioningService.CurrentRights.RightsPerScope[scopeKey];
            List<string> inheritedRights = new List<string>(scopeRights.RightKeys);
            inheritedRights.AddRange(this.roleRights[roleKey]);

            this.userRightsProvisioningService.CurrentRights.ReplaceScopeRights(scopeKey, new ScopeRights
            {
                ScopeId = scopeRights.ScopeId,
                ScopeName = scopeRights.ScopeName,
                ScopeHierarchies = scopeRights.ScopeHierarchies,
                RightKeys = inheritedRights.Distinct().ToList()
            });

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
            List<string> inheritedRights = new List<string>();
            List<string> parentHierarchies = new List<string>();
            foreach (var parent in parents)
            {
                inheritedRights.AddRange(this.userRightsProvisioningService.CurrentRights.RightsPerScope[parent].RightKeys);
                parentHierarchies.AddRange(this.userRightsProvisioningService.CurrentRights.RightsPerScope[parent].ScopeHierarchies);
            }

            List<string> scopeHierarchies = null;
            if (parentHierarchies.Any())
            {
                scopeHierarchies = parentHierarchies.Select(ph => string.Concat(ph, $"/{scopeKey}")).ToList();
            }
            else
            {
                scopeHierarchies = new List<string> { scopeKey };
            }
            
            var scopeRights = new ScopeRights
            {
                ScopeName = scopeKey,
                ScopeHierarchies = scopeHierarchies,
                RightKeys = inheritedRights.Distinct().ToList()
            };

            this.userRightsProvisioningService.CurrentRights.ReplaceScopeRights(scopeKey, scopeRights);                

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
            return Task.CompletedTask;
        }

        public Task UnaffectRoleFromPrincipalOnScopeAsync(string roleKey, Guid principalId, string scopeKey)
        {
            if (this.userRightsProvisioningService.CurrentRights.RightsPerScope.ContainsKey(scopeKey) &&
                this.roleRights.ContainsKey(roleKey))
            {
                var scopeRights = this.userRightsProvisioningService.CurrentRights
                                                                    .RightsPerScope
                                                                    .Select(r => r.Value)
                                                                    .ToList();
                foreach (var scopeRight in scopeRights)
                {
                    scopeRight.RightKeys = scopeRight.RightKeys.Except(this.roleRights[roleKey]);
                }

                this.userRightsProvisioningService.CurrentRights = new RightsResult(scopeRights);
            }

            return Task.CompletedTask;
        }
    }
}
