namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Exceptions;

    public class AuthorizationsProvisioningClient<TContext> : IAuthorizationsProvisioningClient where TContext : DbContext
    {
        private readonly TContext context;

        private readonly Guid currentPrincipalId;

        public AuthorizationsProvisioningClient(TContext context, Guid currentPrincipalId)
        {
            this.context = context;
            this.currentPrincipalId = currentPrincipalId;
        }

        public async Task AddRightsToRoleAsync(string name, string[] rights)
        {
            Data.Role role = await this.context.Set<Data.Role>().FirstOrDefaultAsync(r => r.Name == name);
            if (role == null)
            {
                throw new EntityNotFoundException(name);
            }

            foreach (var right in rights)
            {
                role.Rights.Add(new Data.RoleRight
                {
                    Role = role,
                    Right = new Data.Right
                    {
                        Name = right,
                        CreationBy = this.currentPrincipalId,
                        ModificationBy = this.currentPrincipalId
                    }   
                });
            }

            await this.context.SaveChangesAsync();
        }

        public async Task AffectRoleToPrincipalAsync(string roleName, Guid principalId, string scopeName)
        {
            Data.Role role = await this.context.Set<Data.Role>().FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
            {
                throw new EntityNotFoundException(roleName);
            }

            Data.Scope scope = await this.context.Set<Data.Scope>().FirstOrDefaultAsync(s => s.Name == scopeName);
            if (scope == null)
            {
                throw new EntityNotFoundException(scopeName);
            }

            this.context.Set<Data.Authorization>().Add(new Data.Authorization
            {
                Role = role,
                Scope = scope,
                PrincipalId = principalId,
                CreationBy = this.currentPrincipalId,
                ModificationBy = this.currentPrincipalId
            });

            await this.context.SaveChangesAsync();
        }

        public async Task CreateRightAsync(string name)
        {
            this.context.Set<Data.Right>().Add(new Data.Right
            {
                Name = name,
                CreationBy = this.currentPrincipalId,
                ModificationBy = this.currentPrincipalId
            });

            await this.context.SaveChangesAsync();
        }

        public Task CreateRoleAsync(string name, string[] rights)
        {
            throw new NotImplementedException();
        }

        public Task CreateScopeAsync(string name, string description, params string[] parentnames)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRightAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRoleAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task DeleteScopeAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Model.Right>> GetRightsForRoleAsync(string rolename)
        {
            throw new NotImplementedException();
        }

        public Task<Model.Scope> GetSecurityScopeAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task RemoveRightsFromRoleAsync(string name, string[] rights)
        {
            throw new NotImplementedException();
        }

        public Task RenameRightAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task RenameRoleAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task RenameScopeAsync(string name, string description)
        {
            throw new NotImplementedException();
        }

        public Task UnaffectRoleFromPrincipalAsync(string roleName, Guid principalId, string scopeName)
        {
            throw new NotImplementedException();
        }
    }
}
