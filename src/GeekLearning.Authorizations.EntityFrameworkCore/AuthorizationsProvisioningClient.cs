namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using Exceptions;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using System.Collections.Generic;

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

        public async Task CreateGroupAsync(string groupName, string parentGroupName = null)
        {
            var group = await this.GetEntityAsync<Data.Group>(r => r.Name == groupName);
            if (group == null)
            {
                group = new Data.Group
                {
                    Name = groupName,
                    CreationBy = this.principalIdProvider.PrincipalId,
                    ModificationBy = this.principalIdProvider.PrincipalId,

                };
                var groupEntity = this.context.Set<Data.Group>().Add(group);

                if (parentGroupName != null)
                {
                    await this.CreateGroupAsync(parentGroupName);
                    var parentGoup = await this.GetEntityAsync<Data.Group>(r => r.Name == parentGroupName);
                    this.context.Set<Data.Membership>().Add(new Data.Membership
                    {
                        Principal = group,
                        Group = parentGoup
                    });
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

        public async Task DeleteGroupAsync(string groupName, bool withChildren = true)
        {
            var group = await this.GetEntityAsync<Data.Group>(r => r.Name == groupName);
            if (group != null)
            {
                var memberShips = await this.context.Set<Data.Membership>().Where(m => m.Group.Name == groupName).ToListAsync();
                foreach (var memberShip in memberShips)
                {
                    if (withChildren)
                    {
                        var childGroup = await this.GetEntityAsync<Data.Group>(g => g.Id == memberShip.PrincipalId);
                        if (childGroup != null)
                        {
                            await this.DeleteGroupAsync(childGroup.Name);
                        }
                    }

                    this.context.Set<Data.Membership>().Remove(memberShip);
                }

                this.context.Set<Data.Group>().Remove(group);
            }
        }

        public async Task AddPrincipalToGroup(Guid principalId, string groupName)
        {
            var membership = await this.GetEntityAsync<Data.Membership>(m => m.PrincipalId == principalId && m.Group.Name == groupName);
            if (membership == null)
            {
                await this.CreateGroupAsync(groupName);
                var group = await this.GetEntityAsync<Data.Group>(g => g.Name == groupName);
                this.context.Set<Data.Membership>().Add(new Data.Membership
                {
                    PrincipalId = principalId,
                    Group = group,
                    CreationBy = this.principalIdProvider.PrincipalId,
                    ModificationBy = this.principalIdProvider.PrincipalId
                });
            }
        }

        public Task AddPrincipalsToGroup(IEnumerable<Guid> principalIds, string groupName)
        {
            return Task.WhenAll(principalIds.Select(pId => AddPrincipalToGroup(pId, groupName)));
        }

        public async Task RemovePrincipalFromGroup(Guid principalId, string groupName)
        {
            var membership = await this.GetEntityAsync<Data.Membership>(m => m.PrincipalId == principalId && m.Group.Name == groupName);
            if (membership != null)
            {
                this.context.Set<Data.Membership>().Remove(membership);
            }
        }

        public Task RemovePrincipalsFromGroup(IEnumerable<Guid> principalIds, string groupName)
        {
            return Task.WhenAll(principalIds.Select(pId => RemovePrincipalFromGroup(pId, groupName)));
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
