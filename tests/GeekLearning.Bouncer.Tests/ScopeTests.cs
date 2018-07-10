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
            using (var bouncerFixture = new BouncerFixture())
            {
                await bouncerFixture.AuthorizationsManager
                                            .CreateScopeAsync(
                                                "scope1",
                                                "Description scope 1",
                                                new string[] { "scopeParent1", "scopeParent2" });

                await bouncerFixture.Context.SaveChangesAsync();

                await bouncerFixture.AuthorizationsManager
                                            .CreateScopeAsync(
                                                "scope1",
                                                "Description scope 1",
                                                new string[] { "scopeParent1", "scopeParent2" });

                await bouncerFixture.Context.SaveChangesAsync();

                var scope = bouncerFixture.Context.Scopes
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
            using (var bouncerFixture = new BouncerFixture())
            {
                await bouncerFixture.AuthorizationsManager.DeleteScopeAsync("scope1");

                await bouncerFixture.Context.SaveChangesAsync();

                var parentScope = new Scope
                {
                    Id = Guid.NewGuid(),
                    Name = "scope1",
                    Description = "Scope 1",
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };

                parentScope.Children.Add(
                    new ScopeHierarchy
                    {
                        Parent = parentScope,
                        Child = new Scope
                        {
                            Name = "scopeChild1",
                            Description = "Scope Child 1",
                            CreationBy = bouncerFixture.Context.CurrentUserId,
                            ModificationBy = bouncerFixture.Context.CurrentUserId
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
                            CreationBy = bouncerFixture.Context.CurrentUserId,
                            ModificationBy = bouncerFixture.Context.CurrentUserId
                        }
                    });

                bouncerFixture.Context.Scopes.Add(parentScope);
                
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
                                CreationBy = bouncerFixture.Context.CurrentUserId,
                                ModificationBy = bouncerFixture.Context.CurrentUserId
                            }
                        }
                    },
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };
                
                bouncerFixture.Context.Roles.Add(role);

                bouncerFixture.Context.Authorizations.Add(new Authorization
                {
                    Scope = parentScope,
                    Role = role,
                    PrincipalId = bouncerFixture.Context.CurrentUserId,
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                });

                await bouncerFixture.Context.SaveChangesAsync();

                await bouncerFixture.AuthorizationsManager.DeleteScopeAsync("scope1");

                await bouncerFixture.Context.SaveChangesAsync();

                Assert.Null(bouncerFixture.Context.Scopes
                                                         .FirstOrDefault(r => r.Name == "scope1"));
                Assert.Null(bouncerFixture.Context.Scopes
                                                         .FirstOrDefault(r => r.Name == "scopeChild1"));
                Assert.Null(bouncerFixture.Context.Scopes
                                                         .FirstOrDefault(r => r.Name == "scopeChild2"));

                Assert.False(bouncerFixture.Context.ScopeHierarchies.Any());
                Assert.Null(
                    bouncerFixture.Context.Authorizations
                    .FirstOrDefault(
                        a => a.ScopeId == parentScope.Id &&
                        a.RoleId == role.Id &&
                        a.PrincipalId == bouncerFixture.Context.CurrentUserId));
            }
        }

        [Fact]
        public async Task DeleteChildScope_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                await bouncerFixture.AuthorizationsManager.DeleteScopeAsync("scope1");

                bouncerFixture.Context.SaveChanges();

                var parent = new Scope
                {
                    Name = "scope1",
                    Description = "Scope 1",
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };

                parent.Children.Add(
                    new ScopeHierarchy
                    {
                        Parent = parent,
                        Child = new Scope
                        {
                            Name = "scopeChild1",
                            Description = "Scope Child 1",
                            CreationBy = bouncerFixture.Context.CurrentUserId,
                            ModificationBy = bouncerFixture.Context.CurrentUserId
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
                            CreationBy = bouncerFixture.Context.CurrentUserId,
                            ModificationBy = bouncerFixture.Context.CurrentUserId
                        }
                    });

                bouncerFixture.Context.Scopes.Add(parent);

                bouncerFixture.Context.SaveChanges();

                await bouncerFixture.AuthorizationsManager.DeleteScopeAsync("scopeChild2");

                await bouncerFixture.Context.SaveChangesAsync();

                Assert.NotNull(bouncerFixture.Context.Scopes
                                                         .FirstOrDefault(r => r.Name == "scope1"));
                Assert.NotNull(bouncerFixture.Context.Scopes
                                                         .FirstOrDefault(r => r.Name == "scopeChild1"));
                Assert.Null(bouncerFixture.Context.Scopes
                                                         .FirstOrDefault(r => r.Name == "scopeChild2"));

                Assert.False(bouncerFixture.Context.ScopeHierarchies.Where(x => x.Child.Name == "scopeChild2" || x.Parent.Name == "scopeChild2").Any());
            }
        }
    }
}
