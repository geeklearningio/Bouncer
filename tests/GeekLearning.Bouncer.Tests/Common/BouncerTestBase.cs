namespace GeekLearning.Bouncer.Tests
{
    using GeekLearning.Bouncer.EntityFrameworkCore.Data;
    using System;
    using System.Linq;

    public abstract class BouncerTestBase
    {
        protected Right CreateRight(BouncerTestContext context, string rightName)
        {
            var right = new Right
            {
                Name = rightName,
                CreationBy = context.CurrentUserId,
                ModificationBy = context.CurrentUserId
            };
            context.Rights.Add(right);

            return right;
        }

        public Role CreateRole(BouncerTestContext context, string roleName)
        {
            var role = new Role
            {
                Name = roleName,
                CreationBy = context.CurrentUserId,
                ModificationBy = context.CurrentUserId
            };
            context.Roles.Add(role);

            return role;
        }

        protected RoleRight AddRightToRole(BouncerTestContext context, Right right, Role role)
        {
            var roleRight = new RoleRight
            {
                Right = right,
                Role = role
            };
            context.RoleRights.Add(roleRight);

            return roleRight;
        }

        protected Group CreateGroup(BouncerTestContext context, string groupName)
        {
            var principal = new Principal
            {
                Id = Guid.NewGuid(),
                CreationBy = context.CurrentUserId,
                ModificationBy = context.CurrentUserId
            };
            context.Principals.Add(principal);
            var group = new Group
            {
                Id = principal.Id,
                Name = groupName,
                CreationBy = context.CurrentUserId,
                ModificationBy = context.CurrentUserId
            };
            context.Groups.Add(group);

            return group;
        }

        protected Membership AddPrincipalToGroup(BouncerTestContext context, Guid principalId, Group group)
        {
            var memberShip = new Membership
            {
                PrincipalId = principalId,
                Group = group,
                CreationBy = context.CurrentUserId,
                ModificationBy = context.CurrentUserId
            };
            context.Memberships.Add(memberShip);

            return memberShip;
        }

        protected Authorization CreateAuthorization(BouncerTestContext context, Guid principalId, Role role, EntityFrameworkCore.Data.Scope scope)
        {
            var authorization = new Authorization
            {
                PrincipalId = principalId,
                Role = role,
                Scope = scope,
                CreationBy = context.CurrentUserId,
                ModificationBy = context.CurrentUserId
            };
            context.Authorizations.Add(authorization);

            return authorization;
        }

        protected Scope CreateScope(BouncerTestContext context, string scopeName, params string[] parentScopeNames)
        {
            var scope = new Scope
            {
                Name = scopeName,
                Description = scopeName,
                CreationBy = context.CurrentUserId,
                ModificationBy = context.CurrentUserId
            };
            context.Scopes.Add(scope);

            foreach (var parentScopeName in parentScopeNames)
            {
                var parentScope =
                    context.ChangeTracker
                    .Entries<Scope>()
                    .Select(e => e.Entity)
                    .First(s => s.Name == parentScopeName);

                context.ScopeHierarchies.Add(new ScopeHierarchy
                {
                    Child = scope,
                    Parent = parentScope
                });
            }

            return scope;
        }
    }
}
