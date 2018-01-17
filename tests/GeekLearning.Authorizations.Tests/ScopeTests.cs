﻿namespace GeekLearning.Authorizations.Tests
{
    using EntityFrameworkCore;
    using EntityFrameworkCore.Data;
    using GeekLearning.Authorizations.EntityFrameworkCore.Exceptions;
    using Microsoft.EntityFrameworkCore;
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
        public async Task DeleteScope_ShouldBeOk()
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

                await authorizationsFixture.AuthorizationsManager.DeleteScopeAsync("scope1");

                await authorizationsFixture.Context.SaveChangesAsync();

                Assert.Null(authorizationsFixture.Context.Scopes()
                                                         .FirstOrDefault(r => r.Name == "scope1"));
                Assert.Null(authorizationsFixture.Context.Scopes()
                                                         .FirstOrDefault(r => r.Name == "scopeChild1"));
                Assert.Null(authorizationsFixture.Context.Scopes()
                                                         .FirstOrDefault(r => r.Name == "scopeChild2"));

                Assert.False(authorizationsFixture.Context.ScopeHierarchies().Any());
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
