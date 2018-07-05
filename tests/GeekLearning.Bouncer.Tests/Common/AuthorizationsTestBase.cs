namespace GeekLearning.Bouncer.Tests
{
    using Bouncer.EntityFrameworkCore;
    using System;
    using System.Linq;

    public abstract class AuthorizationsTestBase
    {
        protected EntityFrameworkCore.Data.Right CreateRight(AuthorizationsTestContext context, string rightName)
        {
            var right = new EntityFrameworkCore.Data.Right
            {
                Name = rightName,
                CreationBy = context.CurrentUserId,
                ModificationBy = context.CurrentUserId
            };
            context.Rights().Add(right);

            return right;
        }

        public EntityFrameworkCore.Data.Role CreateRole(AuthorizationsTestContext context, string roleName)
        {
            var role = new EntityFrameworkCore.Data.Role
            {
                Name = roleName,
                CreationBy = context.CurrentUserId,
                ModificationBy = context.CurrentUserId
            };
            context.Roles().Add(role);

            return role;
        }

        protected EntityFrameworkCore.Data.RoleRight AddRightToRole(AuthorizationsTestContext context, EntityFrameworkCore.Data.Right right, EntityFrameworkCore.Data.Role role)
        {
            var roleRight = new EntityFrameworkCore.Data.RoleRight
            {
                Right = right,
                Role = role
            };
            context.RoleRights().Add(roleRight);

            return roleRight;
        }

        protected EntityFrameworkCore.Data.Group CreateGroup(AuthorizationsTestContext context, string groupName)
        {
            var principal = new EntityFrameworkCore.Data.Principal
            {
                Id = Guid.NewGuid(),
                CreationBy = context.CurrentUserId,
                ModificationBy = context.CurrentUserId
            };
            context.Principals().Add(principal);
            var group = new EntityFrameworkCore.Data.Group
            {
                Id = principal.Id,
                Name = groupName,
                CreationBy = context.CurrentUserId,
                ModificationBy = context.CurrentUserId
            };
            context.Groups().Add(group);

            return group;
        }

        protected EntityFrameworkCore.Data.Membership AddPrincipalToGroup(AuthorizationsTestContext context, Guid principalId, EntityFrameworkCore.Data.Group group)
        {
            var memberShip = new EntityFrameworkCore.Data.Membership
            {
                PrincipalId = principalId,
                Group = group,
                CreationBy = context.CurrentUserId,
                ModificationBy = context.CurrentUserId
            };
            context.Memberships().Add(memberShip);

            return memberShip;
        }

        protected EntityFrameworkCore.Data.Authorization CreateAuthorization(AuthorizationsTestContext context, Guid principalId, EntityFrameworkCore.Data.Role role, EntityFrameworkCore.Data.Scope scope)
        {
            var authorization = new EntityFrameworkCore.Data.Authorization
            {
                PrincipalId = principalId,
                Role = role,
                Scope = scope,
                CreationBy = context.CurrentUserId,
                ModificationBy = context.CurrentUserId
            };
            context.Authorizations().Add(authorization);

            return authorization;
        }

        protected EntityFrameworkCore.Data.Scope CreateScope(AuthorizationsTestContext context, string scopeName, params string[] parentScopeNames)
        {
            var scope = new EntityFrameworkCore.Data.Scope
            {
                Name = scopeName,
                Description = scopeName,
                CreationBy = context.CurrentUserId,
                ModificationBy = context.CurrentUserId
            };
            context.Scopes().Add(scope);

            foreach (var parentScopeName in parentScopeNames)
            {
                var parentScope =
                    context.ChangeTracker
                    .Entries<EntityFrameworkCore.Data.Scope>()
                    .Select(e => e.Entity)
                    .First(s => s.Name == parentScopeName);

                context.ScopeHierarchies().Add(new EntityFrameworkCore.Data.ScopeHierarchy
                {
                    Child = scope,
                    Parent = parentScope
                });
            }

            return scope;
        }
    }
}
