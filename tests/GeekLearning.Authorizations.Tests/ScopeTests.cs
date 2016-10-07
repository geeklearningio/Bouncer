using GeekLearning.Authorizations.Data;
using GeekLearning.Authorizations.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace GeekLearning.Authorizations.Tests
{
    public class ScopeTests
    {
        [Fact]
        public async Task CreateScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await authorizationsFixture.AuthorizationsProvisioningClient
                                            .CreateScopeAsync(
                                                "scope1",
                                                "Description scope 1",
                                                new string[] { "scopeParent1", "scopeParent2" });

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
                var parent = new Scope { Name = "scope1", Description = "Scope 1" };

                parent.Children.Add(
                    new ScopeHierarchy
                    {
                        Parent = parent,
                        Child = new Scope { Name = "scopeChild1", Description = "Scope Child 1" }
                    });

                parent.Children.Add(
                    new ScopeHierarchy
                    {
                        Parent = parent,
                        Child = new Scope { Name = "scopeChild2", Description = "Scope Child 2" }
                    });

                authorizationsFixture.Context.Scopes().Add(parent);

                authorizationsFixture.Context.SaveChanges();

                await authorizationsFixture.AuthorizationsProvisioningClient.DeleteScopeAsync("scope1");

                Assert.Null(authorizationsFixture.Context.Scopes()
                                                         .FirstOrDefault(r => r.Name == "scope1"));
                Assert.Null(authorizationsFixture.Context.Scopes()
                                                         .FirstOrDefault(r => r.Name == "scopeChild1"));
                Assert.Null(authorizationsFixture.Context.Scopes()
                                                         .FirstOrDefault(r => r.Name == "scopeChild2"));

                Assert.False(authorizationsFixture.Context.ScopeHierarchies().Any());
            }
        }
    }
}
