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

        private readonly IPrincipalIdProvider principalIdProvider;

        public AuthorizationsProvisioningClient(TContext context, IPrincipalIdProvider principalIdProvider)
        {
            this.context = context;
            this.principalIdProvider = principalIdProvider;
        }

        public async Task AffectRoleToPrincipalOnScopeAsync(string roleKey, Guid principalId, string scopeKey)
        {
            Data.Role role = await this.context.Set<Data.Role>().FirstOrDefaultAsync(r => r.Name == roleKey);
            if (role == null)
            {
                throw new EntityNotFoundException(roleKey);
            }

            Data.Scope scope = await this.context.Set<Data.Scope>().FirstOrDefaultAsync(s => s.Name == scopeKey);
            if (scope == null)
            {
                throw new EntityNotFoundException(scopeKey);
            }

            Data.Principal principal = await this.context.Set<Data.Principal>().FirstOrDefaultAsync(s => s.Id == principalId);
            if (principal == null)
            {
                throw new EntityNotFoundException($"Principal '{principalId}'");
            }

            if (await this.context.Set<Data.Authorization>()
                                  .AnyAsync(
                                    a => a.Role.Name == roleKey &&
                                         a.PrincipalId == principalId &&
                                         a.Scope.Name == scopeKey) == false)
            {
                this.context.Set<Data.Authorization>().Add(new Data.Authorization
                {
                    Role = role,
                    Scope = scope,
                    Principal = principal,
                    CreationBy = this.principalIdProvider.PrincipalId,
                    ModificationBy = this.principalIdProvider.PrincipalId
                });

                await this.context.SaveChangesAsync();
            }
        }

        public async Task UnaffectRoleFromPrincipalOnScopeAsync(string roleKey, Guid principalId, string scopeKey)
        {
            Data.Role role = await this.context.Set<Data.Role>().FirstOrDefaultAsync(r => r.Name == roleKey);
            if (role != null)
            {
                Data.Scope scope = await this.context.Set<Data.Scope>().FirstOrDefaultAsync(s => s.Name == scopeKey);
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

        public async Task CreateRightAsync(string rightKey)
        {
            var right = await this.context.Set<Data.Right>().FirstOrDefaultAsync(r => r.Name == rightKey);
            if (right == null)
            {
                var rightEntity = this.context.Set<Data.Right>().Add(new Data.Right
                {
                    Name = rightKey,
                    CreationBy = this.principalIdProvider.PrincipalId,
                    ModificationBy = this.principalIdProvider.PrincipalId
                });

                await this.context.SaveChangesAsync();
            }
        }

        public async Task CreateRoleAsync(string roleKey, string[] rights)
        {
            var role = await this.context.Set<Data.Role>()
                                         .Include(r => r.Rights)
                                         .FirstOrDefaultAsync(r => r.Name == roleKey);
            if (role == null)
            {
                role = new Data.Role
                {
                    Name = roleKey,
                    CreationBy = this.principalIdProvider.PrincipalId,
                    ModificationBy = this.principalIdProvider.PrincipalId
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

        public async Task CreateScopeAsync(string scopeKey, string description, params string[] parents)
        {
            var scope = await this.context.Set<Data.Scope>()
                                          .Include(s => s.Parents)
                                          .FirstOrDefaultAsync(s => s.Name == scopeKey);

            if (scope == null)
            {
                scope = new Data.Scope
                {
                    Name = scopeKey,
                    Description = description
                };

                this.context.Set<Data.Scope>().Add(scope);
            }

            if (parents != null)
            {
                if (scope.Parents != null)
                {
                    parents = parents.Except(scope.Parents.Select(sp => sp.Parent.Name)).ToArray();
                }

                foreach (var parentName in parents)
                {
                    await this.CreateScopeAsync(parentName, parentName);

                    var parentScope = await this.context.Set<Data.Scope>().FirstOrDefaultAsync(s => s.Name == parentName);

                    this.context.Set<Data.ScopeHierarchy>().Add(new Data.ScopeHierarchy
                    {
                        Child = scope,
                        Parent = parentScope
                    });
                }
            }

            await this.context.SaveChangesAsync();
        }

        public async Task DeleteRightAsync(string rightKey)
        {
            var right = await this.context.Set<Data.Right>().FirstOrDefaultAsync(r => r.Name == rightKey);
            if (right != null)
            {
                this.context.Set<Data.Right>().Remove(right);

                await this.context.SaveChangesAsync();
            }
        }

        public async Task DeleteRoleAsync(string roleKey)
        {
            var role = await this.context.Set<Data.Role>()
                                         .Include(r => r.Rights)
                                         .FirstOrDefaultAsync(r => r.Name == roleKey);
            if (role != null)
            {
                this.context.Set<Data.RoleRight>().RemoveRange(role.Rights);
                this.context.Set<Data.Role>().Remove(role);

                await this.context.SaveChangesAsync();
            }
        }

        public async Task DeleteScopeAsync(string scopeKey)
        {
            var scope = await this.context.Set<Data.Scope>()
                                          .Include(r => r.Children)
                                          .FirstOrDefaultAsync(r => r.Name == scopeKey);
            if (scope != null)
            {
                var childrenScopes = await this.context.Set<Data.Scope>()
                                               .Join(
                                                   this.context.Set<Data.ScopeHierarchy>(),
                                                   s => s.Name,
                                                   sh => sh.Parent.Name,
                                                   (s, sh) => new { Scope = s, ScopeHierarchy = sh })
                                               .Where(r => r.ScopeHierarchy.Parent.Name == scopeKey)
                                               .Select(r => r.ScopeHierarchy.Child)
                                               .ToListAsync();

                this.context.Set<Data.Scope>().RemoveRange(childrenScopes);
                this.context.Set<Data.ScopeHierarchy>()
                            .RemoveRange(
                                await this.context.Set<Data.ScopeHierarchy>()
                                                  .Where(sh => sh.Parent.Name == scopeKey)
                                                  .ToListAsync());
                this.context.Set<Data.Scope>().Remove(scope);

                await this.context.SaveChangesAsync();
            }
        }
    }
}
