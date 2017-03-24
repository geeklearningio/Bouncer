namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using Exceptions;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class AuthorizationsProvisioningClient<TContext> : IAuthorizationsProvisioningClient where TContext : DbContext
    {
        private readonly TContext context;
        private readonly IPrincipalIdProvider principalIdProvider;

        public AuthorizationsProvisioningClient(TContext context, IPrincipalIdProvider principalIdProvider)
        {
            this.context = context;
            this.principalIdProvider = principalIdProvider;
        }

        public async Task AffectRoleToPrincipalOnScopeAsync(string roleName, Guid principalId, string scopeName)
        {
            Data.Role role = await this.GetEntityAsync<Data.Role>(r => r.Name == roleName);
            if (role == null)
            {
                throw new EntityNotFoundException(roleName);
            }

            Data.Scope scope = await this.GetEntityAsync<Data.Scope>(s => s.Name == scopeName);
            if (scope == null)
            {
                throw new EntityNotFoundException(scopeName);
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

        public async Task UnaffectRoleFromPrincipalOnScopeAsync(string roleName, Guid principalId, string scopeName)
        {
            Data.Role role = await this.GetEntityAsync<Data.Role>(r => r.Name == roleName);
            if (role != null)
            {
                Data.Scope scope = await this.GetEntityAsync<Data.Scope>(s => s.Name == scopeName);
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

        public async Task CreateRightAsync(string rightName)
        {
            var right = await this.GetEntityAsync<Data.Right>(r => r.Name == rightName);
            if (right == null)
            {
                var rightEntity = this.context.Set<Data.Right>().Add(new Data.Right
                {
                    Name = rightName,
                    CreationBy = this.principalIdProvider.PrincipalId,
                    ModificationBy = this.principalIdProvider.PrincipalId
                });

                (await SharedQueries.GetModelModificationDateAsync(this.context)).Rights = DateTime.UtcNow;
            }
        }

        public async Task CreateRoleAsync(string roleName, string[] rightNames)
        {
            var role = await this.GetEntityAsync<Data.Role>(r => r.Name == roleName);
            if (role == null)
            {
                role = new Data.Role
                {
                    Name = roleName,
                    CreationBy = this.principalIdProvider.PrincipalId,
                    ModificationBy = this.principalIdProvider.PrincipalId
                };

                this.context.Set<Data.Role>().Add(role);
                (await SharedQueries.GetModelModificationDateAsync(this.context)).Roles = DateTime.UtcNow;
            }

            if (rightNames != null)
            {
                foreach (var rightName in rightNames)
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
                    (await SharedQueries.GetModelModificationDateAsync(this.context)).Rights = DateTime.UtcNow;
                }
            }
        }

        public async Task CreateScopeAsync(string scopeName, string description, params string[] parents)
        {
            var scope = await this.GetEntityAsync<Data.Scope>(s => s.Name == scopeName);

            if (scope == null)
            {
                scope = new Data.Scope
                {
                    Name = scopeName,
                    Description = description,
                    CreationBy = this.principalIdProvider.PrincipalId,
                    ModificationBy = this.principalIdProvider.PrincipalId
                };

                this.context.Set<Data.Scope>().Add(scope);
                (await SharedQueries.GetModelModificationDateAsync(this.context)).Scopes = DateTime.UtcNow;
            }

            if (parents != null)
            {
                foreach (var parentName in parents)
                {
                    await this.CreateScopeAsync(parentName, parentName);

                    var parentScope = await this.GetEntityAsync<Data.Scope>(s => s.Name == parentName);

                    var scopeHierarchy = await this.GetEntityAsync<Data.ScopeHierarchy>(sh => sh.ChildId == scope.Id && sh.ParentId == parentScope.Id);
                    if (scopeHierarchy == null)
                    {
                        this.context.Set<Data.ScopeHierarchy>().Add(new Data.ScopeHierarchy
                        {
                            Child = scope,
                            Parent = parentScope
                        });
                    }
                }
            }
        }

        public async Task DeleteRightAsync(string rightName)
        {
            var right = await this.GetEntityAsync<Data.Right>(r => r.Name == rightName);
            if (right != null)
            {
                this.context.Set<Data.Right>().Remove(right);
                (await SharedQueries.GetModelModificationDateAsync(this.context)).Rights = DateTime.UtcNow;
            }
        }

        public async Task DeleteRoleAsync(string roleName)
        {
            var role = await this.GetEntityAsync<Data.Role>(r => r.Name == roleName);
            if (role != null)
            {
                var roleRights = await this.context.Set<Data.RoleRight>()
                                                   .Where(rr => rr.RoleId == role.Id).ToListAsync();

                this.context.Set<Data.RoleRight>().RemoveRange(roleRights);
                this.context.Set<Data.Role>().Remove(role);
                (await SharedQueries.GetModelModificationDateAsync(this.context)).Roles = DateTime.UtcNow;
            }
        }

        public async Task DeleteScopeAsync(string scopeName)
        {
            var scope = await this.GetEntityAsync<Data.Scope>(s => s.Name == scopeName);
            if (scope != null)
            {
                var childrenScopes = await this.context.Set<Data.Scope>()
                                               .Join(
                                                   this.context.Set<Data.ScopeHierarchy>(),
                                                   s => s.Name,
                                                   sh => sh.Parent.Name,
                                                   (s, sh) => new { Scope = s, ScopeHierarchy = sh })
                                               .Where(r => r.ScopeHierarchy.Parent.Name == scopeName)
                                               .Select(r => r.ScopeHierarchy.Child)
                                               .ToListAsync();

                this.context.Set<Data.Scope>().RemoveRange(childrenScopes);
                this.context.Set<Data.ScopeHierarchy>()
                            .RemoveRange(
                                await this.context.Set<Data.ScopeHierarchy>()
                                                  .Where(sh => sh.Parent.Name == scopeName)
                                                  .ToListAsync());
                this.context.Set<Data.Scope>().Remove(scope);
                (await SharedQueries.GetModelModificationDateAsync(this.context)).Scopes = DateTime.UtcNow;
            }
        }

        private async Task<TEntity> GetEntityAsync<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : class
        {
            var predicate = expression.Compile();
            var local = this.context.ChangeTracker
                .Entries<TEntity>()
                .Select(e => e.Entity)
                .FirstOrDefault(predicate);

            if (local != null)
            {
                return local;
            }

            return await this.context.Set<TEntity>().FirstOrDefaultAsync(expression);
        }
    }
}
