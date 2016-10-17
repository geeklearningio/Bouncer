namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Data.Entity.Extensions;
    using Exceptions;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

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
            Data.Role role = await this.GetEntityAsync<Data.Role>(r => r.Name == roleKey);
            if (role == null)
            {
                throw new EntityNotFoundException(roleKey);
            }

            Data.Scope scope = await this.GetEntityAsync<Data.Scope>(s => s.Name == scopeKey);
            if (scope == null)
            {
                throw new EntityNotFoundException(scopeKey);
            }

            Data.Principal principal = await this.GetEntityAsync<Data.Principal>(s => s.Id == principalId);
            if (principal == null)
            {
                throw new EntityNotFoundException($"Principal '{principalId}'");
            }

            var localAuthorization = context.ChangeTracker.Entries<Data.Authorization>()
                                                          .FirstOrDefault(e => e.Entity.RoleId == role.Id &&
                                                                               e.Entity.ScopeId == scope.Id &&
                                                                               e.Entity.PrincipalId == principalId);
            var authorization = await this.context.Set<Data.Authorization>()
                                                  .FirstOrDefaultAsync(a => a.PrincipalId == principalId &&
                                                                            a.RoleId == role.Id &&
                                                                            a.ScopeId == scope.Id);

            if (localAuthorization != null)
            {
                localAuthorization.State = authorization == null ? EntityState.Added : EntityState.Unchanged;
            }
            else if (authorization == null)
            {
                this.context.Set<Data.Authorization>().Add(new Data.Authorization
                {
                    Role = role,
                    Scope = scope,
                    Principal = principal,
                    CreationBy = this.principalIdProvider.PrincipalId,
                    ModificationBy = this.principalIdProvider.PrincipalId
                });
            }
        }

        public async Task UnaffectRoleFromPrincipalOnScopeAsync(string roleKey, Guid principalId, string scopeKey)
        {
            Data.Role role = await this.GetEntityAsync<Data.Role>(r => r.Name == roleKey);
            if (role != null)
            {
                Data.Scope scope = await this.GetEntityAsync<Data.Scope>(s => s.Name == scopeKey);
                if (scope != null)
                {
                    var localAuthorization = context.ChangeTracker.Entries<Data.Authorization>()
                                                                  .FirstOrDefault(e => e.Entity.RoleId == role.Id &&
                                                                                  e.Entity.ScopeId == scope.Id &&
                                                                                  e.Entity.PrincipalId == principalId);
                    if (localAuthorization != null && localAuthorization.State == EntityState.Added)
                    {
                        localAuthorization.State = EntityState.Unchanged;
                    }

                    var authorization = await this.context.Set<Data.Authorization>()
                                                          .FirstOrDefaultAsync(a => a.PrincipalId == principalId &&
                                                                                    a.RoleId == role.Id &&
                                                                                    a.ScopeId == scope.Id);
                    if (authorization != null)
                    {
                        this.context.Set<Data.Authorization>().Remove(authorization);
                    }
                }
            }
        }

        public async Task CreateRightAsync(string rightKey)
        {
            var right = await this.GetEntityAsync<Data.Right>(r => r.Name == rightKey);
            if (right == null)
            {
                var rightEntity = this.context.Set<Data.Right>().Add(new Data.Right
                {
                    Name = rightKey,
                    CreationBy = this.principalIdProvider.PrincipalId,
                    ModificationBy = this.principalIdProvider.PrincipalId
                });
            }
        }

        public async Task CreateRoleAsync(string roleKey, string[] rights)
        {
            var role = await this.GetEntityAsync<Data.Role>(r => r.Name == roleKey);
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
                foreach (var rightName in rights)
                {
                    await this.CreateRightAsync(rightName);

                    var right = await GetEntityAsync<Data.Right>(r => r.Name == rightName);

                    if (right == null)
                    {
                        throw new InvalidOperationException($"Inconsistency with right : {rightName}. Specified right does not exist.");
                    }

                    role.Rights.Add(new Data.RoleRight
                    {
                        Right = right,
                        Role = role
                    });
                }
            }
        }

        public async Task CreateScopeAsync(string scopeKey, string description, params string[] parents)
        {
            var scope = await this.GetEntityAsync<Data.Scope>(s => s.Name == scopeKey);

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
                foreach (var parentName in parents)
                {
                    await this.CreateScopeAsync(parentName, parentName);

                    var parentScope = await this.GetEntityAsync<Data.Scope>(s => s.Name == parentName);

                    this.context.Set<Data.ScopeHierarchy>().Add(new Data.ScopeHierarchy
                    {
                        Child = scope,
                        Parent = parentScope
                    });
                }
            }
        }

        public async Task DeleteRightAsync(string rightKey)
        {
            var right = await this.GetEntityAsync<Data.Right>(r => r.Name == rightKey);
            if (right != null)
            {
                this.context.Set<Data.Right>().Remove(right);
            }
        }

        public async Task DeleteRoleAsync(string roleKey)
        {
            var role = await this.GetEntityAsync<Data.Role>(r => r.Name == roleKey);
            if (role != null)
            {
                var roleRights = await this.context.Set<Data.RoleRight>()
                                                   .Where(rr => rr.RoleId == role.Id).ToListAsync();

                this.context.Set<Data.RoleRight>().RemoveRange(roleRights);
                this.context.Set<Data.Role>().Remove(role);
            }
        }

        public async Task DeleteScopeAsync(string scopeKey)
        {
            var scope = await this.GetEntityAsync<Data.Scope>(s => s.Name == scopeKey);
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
            }
        }

        private async Task<TEntity> GetEntityAsync<TEntity>(Func<TEntity, bool> predicate) where TEntity : class
        {
            return this.context.ChangeTracker.Entries<TEntity>().Select(e => e.Entity).FirstOrDefault(predicate) ??
                   await this.context.Set<TEntity>().FirstOrDefaultAsync(e => predicate(e));
        }
    }
}
