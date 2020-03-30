namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using Exceptions;
    using GeekLearning.Authorizations.Model.Manager;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class AuthorizationsManager<TContext> : IAuthorizationsManager where TContext : DbContext
    {
        private readonly TContext context;
        private readonly IPrincipalIdProvider principalIdProvider;

        public AuthorizationsManager(TContext context, IPrincipalIdProvider principalIdProvider, IServiceProvider serviceProvider)
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

        public async Task AffectRoleToGroupOnScopeAsync(string roleName, string groupName, string scopeName)
        {
            Data.Group group = await this.GetEntityAsync<Data.Group>(r => r.Name == groupName);
            if (group != null)
            {
                await this.AffectRoleToPrincipalOnScopeAsync(roleName, group.Id, scopeName);
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
                    await this.UnaffectFromPrincipalAsync(principalId, role.Id, scope.Id);
                }
            }
        }

        public async Task UnaffectRoleFromGroupOnScopeAsync(string roleName, string groupName, string scopeName)
        {
            Data.Group group = await this.GetEntityAsync<Data.Group>(r => r.Name == groupName);
            if (group != null)
            {
                await this.UnaffectRoleFromPrincipalOnScopeAsync(roleName, group.Id, scopeName);
            }
        }

        public async Task UnaffectRolesFromGroupAsync(string groupName)
        {
            Data.Group group = await this.GetEntityAsync<Data.Group>(r => r.Name == groupName);
            await this.UnaffectRolesFromGroupAsync(group);
        }

        private async Task UnaffectRolesFromGroupAsync(Data.Group group)
        {
            if (group != null)
            {
                await this.UnaffectFromPrincipalAsync(group.Id);
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

        public async Task DeleteRightAsync(string rightName)
        {
            var right = await this.GetEntityAsync<Data.Right>(r => r.Name == rightName);
            if (right != null)
            {
                this.context.Set<Data.Right>().Remove(right);
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

        public async Task CreateScopeAsync(string scopeName, string description, params string[] parents)
        {
            await CreateScopeInternal(scopeName, description, parents);
        }

        private async Task<Data.Scope> CreateScopeInternal(string scopeName, string description, params string[] parents)
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
                    var parentScope = await this.GetEntityAsync<Data.Scope>(s => s.Name == parentName);
                    if (parentScope == null)
                    {
                        parentScope = await this.CreateScopeInternal(parentName, parentName);
                    }

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

            return scope;
        }

        public async Task UpsertScopeAsync(string scopeName, string description, params string[] parents)
        {
            await this.UpsertScopeInternal(scopeName, description, parents);
        }

        private async Task<Data.Scope> UpsertScopeInternal(string scopeName, string description, params string[] parents)
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
            }
            else
            {
                scope.Name = scopeName;
                scope.Description = description;
                scope.ModificationBy = this.principalIdProvider.PrincipalId;

                var entityState = this.context.ChangeTracker.Entries<Data.Scope>()
                                                            .FirstOrDefault(x => x.Entity == scope)?
                                                            .State;

                if (entityState != null && entityState == EntityState.Unchanged)
                {
                    this.context.Set<Data.Scope>().Update(scope);
                }
            }

            (await SharedQueries.GetModelModificationDateAsync(this.context)).Scopes = DateTime.UtcNow;

            if (parents != null)
            {
                foreach (var parentName in parents)
                {
                    var parentScope = await this.GetEntityAsync<Data.Scope>(s => s.Name == parentName);
                    if (parentScope == null)
                    {
                        parentScope = await this.UpsertScopeInternal(parentName, parentName);
                    }

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

            return scope;
        }

        public async Task DeleteScopeAsync(string scopeName)
        {
            var scope = await this.GetEntityAsync<Data.Scope>(s => s.Name == scopeName);
            await DeleteScopeAsync(scope);
        }

        private async Task DeleteScopeAsync(Data.Scope scope)
        {
            if (scope != null)
            {
                var childrenScopes = await this.context.Set<Data.Scope>()
                                               .Join(
                                                   this.context.Set<Data.ScopeHierarchy>(),
                                                   s => s.Name,
                                                   sh => sh.Parent.Name,
                                                   (s, sh) => new { Scope = s, ScopeHierarchy = sh })
                                               .Where(r => r.ScopeHierarchy.ParentId == scope.Id)
                                               .Select(r => r.ScopeHierarchy.Child)
                                               .ToListAsync();

                foreach (var childrenScope in childrenScopes)
                {
                    await DeleteScopeAsync(childrenScope);
                }

                this.context
                    .ScopeHierarchies()
                    .RemoveRange(await this.context.ScopeHierarchies()
                        .Where(sh => sh.ParentId == scope.Id)
                        .ToListAsync());

                this.context
                    .ScopeHierarchies()
                    .RemoveRange(await this.context.ScopeHierarchies()
                        .Where(sh => sh.ChildId == scope.Id)
                        .ToListAsync());

                this.context
                    .Authorizations()
                    .RemoveRange(await this.context.Authorizations()
                        .Where(a => a.ScopeId == scope.Id)
                        .ToListAsync());

                this.context.Scopes().Remove(scope);
                (await SharedQueries.GetModelModificationDateAsync(this.context)).Scopes = DateTime.UtcNow;
            }
        }

        public async Task LinkScopeAsync(string parentScopeName, string childScopeName)
        {
            var parentScope = await this.GetEntityAsync<Data.Scope>(s => s.Name == parentScopeName);
            var childScope = await this.GetEntityAsync<Data.Scope>(s => s.Name == childScopeName);

            var existingLink = await this.GetEntityAsync<Data.ScopeHierarchy>(s => s.ChildId == childScope.Id && s.ParentId == parentScope.Id);

            if (existingLink == null)
            {
                this.context.Set<Data.ScopeHierarchy>().Add(new Data.ScopeHierarchy
                {
                    Child = childScope,
                    Parent = parentScope
                });
                parentScope.ModificationBy = principalIdProvider.PrincipalId;
                parentScope.ModificationDate = DateTime.UtcNow;
                childScope.ModificationBy = principalIdProvider.PrincipalId;
                childScope.ModificationDate = DateTime.UtcNow;

                (await SharedQueries.GetModelModificationDateAsync(this.context)).Scopes = DateTime.UtcNow;
            }
        }

        public async Task UnlinkScopeAsync(string parentScopeName, string childScopeName)
        {
            var parentScope = await this.GetEntityAsync<Data.Scope>(s => s.Name == parentScopeName);
            var childScope = await this.GetEntityAsync<Data.Scope>(s => s.Name == childScopeName);

            var existingLink = await this.GetEntityAsync<Data.ScopeHierarchy>(s => s.ChildId == childScope.Id && s.ParentId == parentScope.Id);

            if (existingLink != null)
            {
                this.context.Set<Data.ScopeHierarchy>().Remove(existingLink);
                parentScope.ModificationBy = principalIdProvider.PrincipalId;
                parentScope.ModificationDate = DateTime.UtcNow;
                childScope.ModificationBy = principalIdProvider.PrincipalId;
                childScope.ModificationDate = DateTime.UtcNow;

                (await SharedQueries.GetModelModificationDateAsync(this.context)).Scopes = DateTime.UtcNow;
            }
        }

        public async Task<IGroup> CreateGroupAsync(string groupName, string parentGroupName = null)
        {
            var group = await this.GetEntityAsync<Data.Group>(r => r.Name == groupName);
            if (group == null)
            {
                var principal = new Data.Principal
                {
                    Id = Guid.NewGuid(),
                    CreationBy = this.principalIdProvider.PrincipalId,
                    ModificationBy = this.principalIdProvider.PrincipalId
                };
                group = new Data.Group(principal) { Name = groupName };

                this.context.Set<Data.Group>().Add(group);

                if (parentGroupName != null)
                {
                    await this.CreateGroupAsync(parentGroupName);
                    var parentGoup = await this.GetEntityAsync<Data.Group>(r => r.Name == parentGroupName);
                    this.context.Set<Data.Membership>().Add(new Data.Membership
                    {
                        CreationBy = principal.CreationBy,
                        ModificationBy = principal.ModificationBy,
                        PrincipalId = group.Id,
                        Group = parentGoup
                    });
                }
            }
            return group;
        }

        public async Task DeleteGroupAsync(string groupName, bool withChildren = true)
        {
            var group = await this.GetEntityAsync<Data.Group>(g => g.Name == groupName);
            await DeleteGroupAsync(group, withChildren: withChildren);
        }

        public async Task DeleteGroupAsync(Guid groupId, bool withChildren = true)
        {
            var group = await this.GetEntityAsync<Data.Group>(g => g.Id == groupId);
            await DeleteGroupAsync(group, withChildren: withChildren);
        }

        private async Task DeleteGroupAsync(Data.Group group, bool withChildren = true)
        {
            if (group != null)
            {
                var memberShips = await this.context.Set<Data.Membership>().Where(m => m.GroupId == group.Id).ToListAsync();
                foreach (var memberShip in memberShips)
                {
                    if (withChildren)
                    {
                        var childGroup = await this.GetEntityAsync<Data.Group>(g => g.Id == memberShip.PrincipalId);
                        if (childGroup != null)
                        {
                            await this.DeleteGroupAsync(childGroup);
                        }
                    }

                    this.context.Set<Data.Membership>().Remove(memberShip);
                }

                await this.UnaffectRolesFromGroupAsync(group);

                this.context.Set<Data.Group>().Remove(group);

                var principal = group.Principal;
                if (principal == null)
                {
                    principal = await this.GetEntityAsync<Data.Principal>(x => x.Id == group.Id);
                }

                this.context.Set<Data.Principal>().Remove(principal);
            }
        }

        public async Task AddPrincipalToGroupAsync(Guid principalId, Guid groupId)
        {
            var membership = await this.GetEntityAsync<Data.Membership>(m => m.PrincipalId == principalId && m.Group != null && m.GroupId == groupId);
            if (membership == null)
            {
                var group = await this.GetEntityAsync<Data.Group>(g => g.Id == groupId);
                this.context.Set<Data.Membership>().Add(new Data.Membership
                {
                    PrincipalId = principalId,
                    GroupId = groupId,
                    CreationBy = this.principalIdProvider.PrincipalId,
                    ModificationBy = this.principalIdProvider.PrincipalId
                });
            }
            else
            {
                this.CancelEntityEntryForState<Data.Membership>(membership, EntityState.Deleted);
            }
        }

        public async Task AddPrincipalToGroupAsync(Guid principalId, string groupName)
        {
            var membership = await this.GetEntityAsync<Data.Membership>(m => m.PrincipalId == principalId && m.Group != null && m.Group.Name == groupName);
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
            else
            {
                this.CancelEntityEntryForState<Data.Membership>(membership, EntityState.Deleted);
            }
        }

        public Task AddPrincipalsToGroupAsync(IEnumerable<Guid> principalIds, string groupName)
        {
            return Task.WhenAll(principalIds.Select(pId => AddPrincipalToGroupAsync(pId, groupName)));
        }

        public async Task AddGroupToGroupAsync(string childGroupName, string parentGroupName)
        {
            await this.CreateGroupAsync(childGroupName);
            var childGroup = await this.GetEntityAsync<Data.Group>(g => g.Name == childGroupName);
            var membership = await this.GetEntityAsync<Data.Membership>(m => m.PrincipalId == childGroup.Id && m.Group.Name == parentGroupName);
            if (membership == null)
            {
                await this.CreateGroupAsync(parentGroupName);
                var parentGroup = await this.GetEntityAsync<Data.Group>(g => g.Name == parentGroupName);
                this.context.Set<Data.Membership>().Add(new Data.Membership
                {
                    PrincipalId = childGroup.Id,
                    Group = parentGroup,
                    CreationBy = this.principalIdProvider.PrincipalId,
                    ModificationBy = this.principalIdProvider.PrincipalId
                });
            }
        }

        public async Task<IGroup> GetGroupAsync(Guid groupId)
        {
            return await this.GetEntityAsync<Data.Group>(g => g.Id == groupId);
        }

        public async Task<IGroup> GetGroupAsync(string groupName)
        {
            return await this.GetEntityAsync<Data.Group>(g => g.Name == groupName);
        }

        public async Task<IList<Guid>> GetGroupMembersAsync(Guid groupId)
        {
            List<Guid> principalIds = new List<Guid>();
            var groupMembers = await this.context.Memberships()
                .Where(m => m.GroupId == groupId)
                .ToListAsync();
            principalIds.AddRange(groupMembers.Select(gm => gm.PrincipalId));
            foreach (var groupMember in groupMembers)
            {
                principalIds.AddRange(await GetGroupMembersAsync(groupMember.PrincipalId));
            }

            return principalIds;
        }

        public async Task<IList<Guid>> GetGroupMembersAsync(string groupName)
        {
            return await this.GetGroupMembersAsync(
                await this.context.Groups().Where(g => g.Name == groupName).Select(g => g.Id).FirstOrDefaultAsync());
        }

        public async Task<IDictionary<string, IList<Guid>>> GetGroupMembersAsync(params string[] groupNames)
        {
            Dictionary<string, IList<Guid>> groupMembers = new Dictionary<string, IList<Guid>>();
            // To be improved with a single query
            foreach (var groupName in groupNames)
            {
                groupMembers[groupName] = await this.GetGroupMembersAsync(groupName);
            }

            return groupMembers;
        }

        public async Task<IList<Guid>> HasMembershipAsync(IEnumerable<Guid> principalIds, params string[] groupNames)
        {
            return await this.context
                .Memberships()
                .Where(m => principalIds.Contains(m.PrincipalId) && groupNames.Contains(m.Group.Name))
                .Select(m => m.PrincipalId)
                .ToListAsync();
        }

        public async Task RemovePrincipalFromGroupAsync(Guid principalId, Guid groupId)
        {
            var membership = await this.GetEntityAsync<Data.Membership>(m => m.PrincipalId == principalId && m.GroupId == groupId);
            if (membership != null)
            {
                this.context.Set<Data.Membership>().Remove(membership);

                var group = await this.GetEntityAsync<Data.Group>(g => g.Id == groupId);
            }
            else
            {
                this.CancelEntityEntryForState<Data.Membership>(membership, EntityState.Added);
            }
        }

        public async Task RemovePrincipalFromGroupAsync(Guid principalId, string groupName)
        {
            var membership = await this.GetEntityAsync<Data.Membership>(m => m.PrincipalId == principalId && m.Group != null && m.Group.Name == groupName);
            if (membership != null)
            {
                this.context.Set<Data.Membership>().Remove(membership);
            }
            else
            {
                this.CancelEntityEntryForState<Data.Membership>(membership, EntityState.Added);
            }
        }

        public Task RemovePrincipalsFromGroupAsync(IEnumerable<Guid> principalIds, string groupName)
        {
            return Task.WhenAll(principalIds.Select(pId => RemovePrincipalFromGroupAsync(pId, groupName)));
        }

        public async Task RemoveAllPrincipalsFromGroupAsync(string groupName)
        {
            await this.RemovePrincipalsFromGroupAsync(await this.GetGroupMembersAsync(groupName), groupName);
        }

        private async Task UnaffectFromPrincipalAsync(Guid principalId, Guid? roleId = null, Guid? scopeId = null)
        {
            var localAuthorizationQuery = context.ChangeTracker.Entries<Data.Authorization>()
                .Where(e => e.Entity.PrincipalId == principalId);
            var authorizationQuery = this.context.Set<Data.Authorization>()
                .Where(a => a.PrincipalId == principalId);

            if (roleId.HasValue)
            {
                localAuthorizationQuery = localAuthorizationQuery
                    .Where(e => e.Entity.RoleId == roleId.Value);
                authorizationQuery = authorizationQuery
                    .Where(a => a.RoleId == roleId.Value);
            }

            if (scopeId.HasValue)
            {
                localAuthorizationQuery = localAuthorizationQuery
                    .Where(e => e.Entity.ScopeId == scopeId.Value);
                authorizationQuery = authorizationQuery
                    .Where(a => a.ScopeId == scopeId.Value);
            }

            var localAuthorization = localAuthorizationQuery.FirstOrDefault();
            if (localAuthorization != null && localAuthorization.State == EntityState.Added)
            {
                localAuthorization.State = EntityState.Unchanged;
            }

            var authorizations = await authorizationQuery.ToListAsync();
            if (authorizations != null)
            {
                foreach (var authorization in authorizations)
                {
                    this.context.Set<Data.Authorization>().Remove(authorization);
                }
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

        private void CancelEntityEntryForState<TEntity>(TEntity entity, EntityState state) where TEntity : class
        {
            var entityEntry = this.context
                .ChangeTracker
                .Entries<TEntity>()
                .Where(ee => ee.Entity == entity && ee.State == state)
                .FirstOrDefault();
            if (entityEntry != null)
            {
                entityEntry.State = EntityState.Unchanged;
            }
        }
    }
}
