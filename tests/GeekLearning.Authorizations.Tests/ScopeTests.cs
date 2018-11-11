namespace GeekLearning.Authorizations.Tests
{
    using EntityFrameworkCore;
    using EntityFrameworkCore.Data;
    using GeekLearning.Authorizations.EntityFrameworkCore.Exceptions;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

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
                Assert.True(parentKeys.Contains("scopeParent1"));
                Assert.True(parentKeys.Contains("scopeParent2"));
            }
        }

        [Fact]
        public async Task UpsertScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                // Common execution
                async Task<Scope> ExecuteUpsert(string description, string[] parents)
                {
                    await authorizationsFixture.AuthorizationsManager
                                               .UpsertScopeAsync("scope1", description, parents);

                    await authorizationsFixture.Context.SaveChangesAsync();

                    return authorizationsFixture.Context.Scopes()
                                                        .Include(r => r.Parents)
                                                        .Include(r => r.Children)
                                                        .FirstOrDefault(r => r.Name == "scope1");
                }

                // Common Assert
                void TestAssertion(Scope scopeInTest, string description, string[] parents)
                {
                    Assert.NotNull(scopeInTest);
                    Assert.Equal(scopeInTest.Description, description);

                    var parentKeys = scopeInTest.Parents.Select(r => r.Parent.Name);
                    foreach (var parent in parents)
                    {
                        Assert.True(parentKeys.Contains(parent));
                    }
                }

                // Create the scope1
                var scopeDescription = "Description scope 1";
                var initialParents = new string[] { "scopeParent1", "scopeParent2" };

                var scope = await ExecuteUpsert(scopeDescription, initialParents);
                TestAssertion(scope, scopeDescription, initialParents);

                // First update
                scopeDescription = "Updated description scope 1";
                var updatedParents = new string[] { "scopeParent1" };

                scope = await ExecuteUpsert(scopeDescription, updatedParents);
                TestAssertion(scope, scopeDescription, initialParents.Concat(updatedParents).ToArray());

                // Second update
                var secondParentUpdate = new string[] { "anotherParent" };

                scope = await ExecuteUpsert(scopeDescription, secondParentUpdate);
                TestAssertion(
                    scope, 
                    scopeDescription, 
                    initialParents.Union(updatedParents)
                                  .Union(secondParentUpdate)
                                  .ToArray());
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
