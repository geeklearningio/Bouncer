namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using System;
    using System.Linq;
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

        public async Task AffectRoleToPrincipalOnScopeAsync(string roleName, Guid principalId, string scopeName)
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

        public async Task UnaffectRoleFromPrincipalOnScopeAsync(string roleName, Guid principalId, string scopeName)
        {
            Data.Role role = await this.context.Set<Data.Role>().FirstOrDefaultAsync(r => r.Name == roleName);
            if (role != null)
            {
                Data.Scope scope = await this.context.Set<Data.Scope>().FirstOrDefaultAsync(s => s.Name == scopeName);
                if (scope != null)
                {
                    var authorization = await this.context.Set<Data.Authorization>()
                                                          .FirstOrDefaultAsync(a => a.PrincipalId == principalId &&
                                                                                    a.RoleId == role.Id &&
                                                                                    a.ScopeId == scope.Id);
                    if (authorization != null)
                    {
                        this.context.Set<Data.Authorization>().Remove(authorization);

                        await this.context.SaveChangesAsync();
                    }
                }
            }
        }

        public async Task CreateRightAsync(string name)
        {
            var right = await this.context.Set<Data.Right>().FirstOrDefaultAsync(r => r.Name == name);
            if (right == null)
            {
                var rightEntity = this.context.Set<Data.Right>().Add(new Data.Right
                {
                    Name = name,
                    CreationBy = this.currentPrincipalId,
                    ModificationBy = this.currentPrincipalId
                });

                await this.context.SaveChangesAsync();
            }
        }

        public async Task CreateRoleAsync(string name, string[] rights)
        {
            var role = await this.context.Set<Data.Role>()
                                         .Include(r => r.Rights)
                                         .FirstOrDefaultAsync(r => r.Name == name);
            if (role == null)
            {
                role = new Data.Role
                {
                    Name = name,
                    CreationBy = this.currentPrincipalId,
                    ModificationBy = this.currentPrincipalId
                };

                this.context.Set<Data.Role>().Add(role);
            }

            if (rights != null)
            {
                foreach (var rightName in rights.Except(role.Rights.Select(rr => rr.Right.Name)))
                {
                    await this.CreateRightAsync(rightName);

                    var right = await this.context.Set<Data.Right>().FirstAsync(r => r.Name == rightName);
                    role.Rights.Add(new Data.RoleRight
                    {
                        Right = right,
                        Role = role
                    });
                }
            }

            await this.context.SaveChangesAsync();
        }

        public async Task CreateScopeAsync(string name, string description, params string[] parents)
        {
            var scope = await this.context.Set<Data.Scope>()
                                          .Include(s => s.Parents)
                                          .FirstOrDefaultAsync(s => s.Name == name);

            if (scope == null)
            {
                scope = new Data.Scope
                {
                    Name = name,
                    Description = description
                };

                this.context.Set<Data.Scope>().Add(scope);
            }

            if (parents != null)
            {
                foreach (var parentName in parents.Except(scope.Parents.Select(sp => sp.Name)))
                {
                    await this.CreateScopeAsync(parentName, parentName);

                    var parentScope = await this.context.Set<Data.Scope>().FirstAsync(s => s.Name == parentName);
                    scope.Parents.Add(parentScope);
                }
            }

            await this.context.SaveChangesAsync();
        }

        public async Task DeleteRightAsync(string name)
        {
            var right = await this.context.Set<Data.Right>().FirstOrDefaultAsync(r => r.Name == name);
            if (right != null)
            {
                this.context.Set<Data.Right>().Remove(right);

                await this.context.SaveChangesAsync();
            }
        }

        public async Task DeleteRoleAsync(string name)
        {
            var role = await this.context.Set<Data.Role>()
                                         .Include(r => r.Rights)
                                         .FirstOrDefaultAsync(r => r.Name == name);
            if (role != null)
            {
                this.context.Set<Data.RoleRight>().RemoveRange(role.Rights);
                this.context.Set<Data.Role>().Remove(role);

                await this.context.SaveChangesAsync();
            }
        }

        public async Task DeleteScopeAsync(string name)
        {
            var scope = await this.context.Set<Data.Scope>()
                                          .Include(r => r.Children)
                                          .FirstOrDefaultAsync(r => r.Name == name);
            if (scope != null)
            {
                this.context.Set<Data.Scope>().RemoveRange(scope.Children);
                this.context.Set<Data.Scope>().Remove(scope);

                await this.context.SaveChangesAsync();
            }
        }
    }
}
