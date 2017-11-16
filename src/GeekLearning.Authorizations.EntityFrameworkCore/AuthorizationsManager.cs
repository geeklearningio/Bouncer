﻿namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using Exceptions;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using GeekLearning.Authorizations.Event;
    using GeekLearning.Authorizations.Events.Model;
    using Microsoft.Extensions.DependencyInjection;
    using GeekLearning.Authorizations.Model.Manager;

    public class AuthorizationsManager<TContext> : IAuthorizationsManager where TContext : DbContext
    {
        private readonly TContext context;
        private readonly IPrincipalIdProvider principalIdProvider;
        private readonly IEventQueuer eventQueuer;

        public AuthorizationsManager(TContext context, IPrincipalIdProvider principalIdProvider, IServiceProvider serviceProvider)
        {
            this.context = context;
            this.principalIdProvider = principalIdProvider;
            this.eventQueuer = serviceProvider.GetService<IEventQueuer>();
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

            this.QueueEvent(new AffectRoleToPrincipalOnScope(principalId, roleName, scopeName));
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

                    this.QueueEvent(new UnaffectRoleFromPrincipalOnScope(principalId, roleName, scopeName));
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

            this.QueueEvent(new CreateScope(scopeName));
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

                foreach (var childrenScope in childrenScopes)
                {
                    await DeleteScopeAsync(childrenScope.Name);
                }

                this.context.Set<Data.ScopeHierarchy>()
                            .RemoveRange(
                                await this.context.Set<Data.ScopeHierarchy>()
                                                  .Where(sh => sh.Parent.Name == scopeName)
                                                  .ToListAsync());
                this.context.Set<Data.Scope>().Remove(scope);
                (await SharedQueries.GetModelModificationDateAsync(this.context)).Scopes = DateTime.UtcNow;

                this.QueueEvent(new DeleteScope(scopeName));
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

        public async Task AddPrincipalToGroupAsync(Guid principalId, string groupName)
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

                this.QueueEvent(new AddPrincipalToGroup(principalId, groupName));
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

                this.QueueEvent(new AddPrincipalToGroup(childGroup.Id, parentGroup.Name));
            }
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

        public async Task RemovePrincipalFromGroupAsync(Guid principalId, string groupName)
        {
            var membership = await this.GetEntityAsync<Data.Membership>(m => m.PrincipalId == principalId && m.Group.Name == groupName);
            if (membership != null)
            {
                this.context.Set<Data.Membership>().Remove(membership);

                this.QueueEvent(new RemovePrincipalFromGroup(principalId, groupName));
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

        private void QueueEvent(EventBase authorizationsEvent)
        {
            if (this.eventQueuer != null)
            {
                this.eventQueuer.QueueEvent(authorizationsEvent);
            }
        }
    }
}