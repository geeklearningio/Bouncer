namespace GeekLearning.Bouncer.Tests
{
    using EntityFrameworkCore;
    using EntityFrameworkCore.Data;
    using Bouncer.EntityFrameworkCore.Exceptions;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;
    using GeekLearning.Bouncer.EntityFrameworkCore.Data.Extensions;

    public class ScopeTests
    {
        [Fact]
        public async Task CreateScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await authorizationsFixture.AuthorizationsManager
                                            .CreateScopeAsync(
                                                "scope1",
                                                "Description scope 1",
                                                new string[] { "scopeParent1", "scopeParent2" });

                await authorizationsFixture.Context.SaveChangesAsync();

                await authorizationsFixture.AuthorizationsManager
                                            .CreateScopeAsync(
                                                "scope1",
                                                "Description scope 1",
                                                new string[] { "scopeParent1", "scopeParent2" });

                await authorizationsFixture.Context.SaveChangesAsync();

                var scope = authorizationsFixture.Context.Scopes()
                                                         .Include(r => r.Parents)
                                                         .Include(r => r.Children)
                                                         .FirstOrDefault(r => r.Name == "scope1");
                Assert.NotNull(scope);

                var parentKeys = scope.Parents.Select(r => r.Parent.Name);
                Assert.Contains("scopeParent1", parentKeys);
                Assert.Contains("scopeParent2", parentKeys);
            }
        }

        [Fact]
        public async Task DeleteScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await authorizationsFixture.AuthorizationsManager.DeleteScopeAsync("scope1");

                await authorizationsFixture.Context.SaveChangesAsync();

                var parentScope = new Scope
                {
                    Id = Guid.NewGuid(),
                    Name = "scope1",
                    Description = "Scope 1",
                    CreationBy = authorizationsFixture.Context.CurrentUserId,
                    ModificationBy = authorizationsFixture.Context.CurrentUserId
                };

                parentScope.Children.Add(
                    new ScopeHierarchy
                    {
                        Parent = parentScope,
                        Child = new Scope
                        {
                            Name = "scopeChild1",
                            Description = "Scope Child 1",
                            CreationBy = authorizationsFixture.Context.CurrentUserId,
                            ModificationBy = authorizationsFixture.Context.CurrentUserId
                        }
                    });

                parentScope.Children.Add(
                    new ScopeHierarchy
                    {
                        Parent = parentScope,
                        Child = new Scope
                        {
                            Name = "scopeChild2",
                            Description = "Scope Child 2",
                            CreationBy = authorizationsFixture.Context.CurrentUserId,
                            ModificationBy = authorizationsFixture.Context.CurrentUserId
                        }
                    });

                authorizationsFixture.Context.Scopes().Add(parentScope);
                
                var role = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "role1",
                    Rights = new List<RoleRight>
                    {
                        new RoleRight
                        {
                            Right = new Right
                            {
                                Name = "right1",
                                CreationBy = authorizationsFixture.Context.CurrentUserId,
                                ModificationBy = authorizationsFixture.Context.CurrentUserId
                            }
                        }
                    },
                    CreationBy = authorizationsFixture.Context.CurrentUserId,
                    ModificationBy = authorizationsFixture.Context.CurrentUserId
                };
                
                authorizationsFixture.Context.Roles().Add(role);

                authorizationsFixture.Context.Authorizations().Add(new Authorization
                {
                    Scope = parentScope,
                    Role = role,
                    PrincipalId = authorizationsFixture.Context.CurrentUserId,
                    CreationBy = authorizationsFixture.Context.CurrentUserId,
                    ModificationBy = authorizationsFixture.Context.CurrentUserId
                });

                await authorizationsFixture.Context.SaveChangesAsync();

                await authorizationsFixture.AuthorizationsManager.DeleteScopeAsync("scope1");

                await authorizationsFixture.Context.SaveChangesAsync();

                Assert.Null(authorizationsFixture.Context.Scopes()
                                                         .FirstOrDefault(r => r.Name == "scope1"));
                Assert.Null(authorizationsFixture.Context.Scopes()
                                                         .FirstOrDefault(r => r.Name == "scopeChild1"));
                Assert.Null(authorizationsFixture.Context.Scopes()
                                                         .FirstOrDefault(r => r.Name == "scopeChild2"));

                Assert.False(authorizationsFixture.Context.ScopeHierarchies().Any());
                Assert.Null(
                    authorizationsFixture.Context.Authorizations()
                    .FirstOrDefault(
                        a => a.ScopeId == parentScope.Id &&
                        a.RoleId == role.Id &&
                        a.PrincipalId == authorizationsFixture.Context.CurrentUserId));
            }
        }

        [Fact]
        public async Task DeleteChildScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await authorizationsFixture.AuthorizationsManager.DeleteScopeAsync("scope1");

                authorizationsFixture.Context.SaveChanges();

                var parent = new Scope
                {
                    Name = "scope1",
                    Description = "Scope 1",
                    CreationBy = authorizationsFixture.Context.CurrentUserId,
                    ModificationBy = authorizationsFixture.Context.CurrentUserId
                };

                parent.Children.Add(
                    new ScopeHierarchy
                    {
                        Parent = parent,
                        Child = new Scope
                        {
                            Name = "scopeChild1",
                            Description = "Scope Child 1",
                            CreationBy = authorizationsFixture.Context.CurrentUserId,
                            ModificationBy = authorizationsFixture.Context.CurrentUserId
                        }
                    });

                parent.Children.Add(
                    new ScopeHierarchy
                    {
                        Parent = parent,
                        Child = new Scope
                        {
                            Name = "scopeChild2",
                            Description = "Scope Child 2",
                            CreationBy = authorizationsFixture.Context.CurrentUserId,
                            ModificationBy = authorizationsFixture.Context.CurrentUserId
                        }
                    });

                authorizationsFixture.Context.Scopes().Add(parent);

                authorizationsFixture.Context.SaveChanges();

                await authorizationsFixture.AuthorizationsManager.DeleteScopeAsync("scopeChild2");

                await authorizationsFixture.Context.SaveChangesAsync();

                Assert.NotNull(authorizationsFixture.Context.Scopes()
                                                         .FirstOrDefault(r => r.Name == "scope1"));
                Assert.NotNull(authorizationsFixture.Context.Scopes()
                                                         .FirstOrDefault(r => r.Name == "scopeChild1"));
                Assert.Null(authorizationsFixture.Context.Scopes()
                                                         .FirstOrDefault(r => r.Name == "scopeChild2"));

                Assert.False(authorizationsFixture.Context.ScopeHierarchies().Where(x => x.Child.Name == "scopeChild2" || x.Parent.Name == "scopeChild2").Any());
            }
        }
    }
}
