namespace GeekLearning.Authorizations.Tests
{
    using GeekLearning.Authorizations.Data;
    using GeekLearning.Authorizations.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Xunit;
    using GeekLearning.Authorizations.Model;

    public class AuthorizationsTest
    {
        [Fact]
        public async Task AffectRoleOnScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                authorizationsFixture.Context.Roles().Add(new Role { Name = "role1" });

                authorizationsFixture.Context.Scopes().Add(new Scope { Name = "scope1", Description = "Scope 1" });

                authorizationsFixture.Context.SaveChanges();

                await authorizationsFixture.AuthorizationsProvisioningClient
                                           .AffectRoleToPrincipalOnScopeAsync(
                                                "role1",
                                                authorizationsFixture.Context.CurrentUserId,
                                                "scope1");

                var authorization = authorizationsFixture.Context.Authorizations()
                                                                 .Include(a => a.Scope)
                                                                 .Include(a => a.Role)
                                                                 .FirstOrDefault(a => a.PrincipalId == authorizationsFixture.Context.CurrentUserId);

                Assert.NotNull(authorization);
                Assert.Equal("role1", authorization.Role.Name);
                Assert.Equal("scope1", authorization.Scope.Name);
            }
        }

        [Fact]
        public async Task UnaffectRoleOnScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                authorizationsFixture.Context.Roles().Add(new Role { Name = "role1" });

                authorizationsFixture.Context.Scopes().Add(new Scope { Name = "scope1", Description = "Scope 1" });

                authorizationsFixture.Context.SaveChanges();

                await authorizationsFixture.AuthorizationsProvisioningClient
                                           .AffectRoleToPrincipalOnScopeAsync(
                                                "role1",
                                                authorizationsFixture.Context.CurrentUserId,
                                                "scope1");

                await authorizationsFixture.AuthorizationsProvisioningClient
                                           .UnaffectRoleFromPrincipalOnScopeAsync(
                                                "role1",
                                                authorizationsFixture.Context.CurrentUserId,
                                                "scope1");

                var authorization = authorizationsFixture.Context.Authorizations()
                                                                 .Include(a => a.Scope)
                                                                 .Include(a => a.Role)
                                                                 .FirstOrDefault(a => a.PrincipalId == authorizationsFixture.Context.CurrentUserId);

                Assert.Null(authorization);
            }
        }

        [Fact]
        public async Task GetRightsOnScope_ShouldBeOk()
        {
            RightsResult rightsResult = new RightsResult(new List<ScopeRights>
            {
                new ScopeRights
                {
                    ScopeId = Guid.NewGuid(),
                    ScopeName = "Scope1",
                    InheritedRightKeys = new string[] { "right1", "right2" },
                    ExplicitRightKeys = new string[] { "right1", "right2" },
                    ScopeHierarchy = "Scope1"
                },
                new ScopeRights
                {
                    ScopeId = Guid.NewGuid(),
                    ScopeName = "Scope1_Child1",
                    InheritedRightKeys = new string[] { "right1", "right2", "right3" },
                    ExplicitRightKeys = new string[] { "right3" },
                    ScopeHierarchy = "Scope1/Scope1_Child1"
                },
                new ScopeRights
                {
                    ScopeId = Guid.NewGuid(),
                    ScopeName = "Scope2_Child1",
                    InheritedRightKeys = new string[] { "right4" },
                    ExplicitRightKeys = new string[] { "right4" },
                    ScopeHierarchy = "Scope2/Scope2_Child1"
                }
            });

            using (var authorizationsFixture = new AuthorizationsFixture(rightsResult))
            {
                var result = await authorizationsFixture.AuthorizationsClient.GetRightsAsync("Scope1_Child1", withChildren: true);

                Assert.True(result.HasRightOnScope("right3", "Scope1_Child1"));
                Assert.True(result.HasRightOnScope("right1", "Scope1_Child1"));
                Assert.False(result.HasRightOnScope("right3", "Scope1"));
                Assert.True(result.HasAnyRightUnderScope("Scope1"));
                Assert.True(result.HasAnyRightUnderScope("Scope1_Child1"));
                Assert.True(result.HasAnyRightUnderScope("Scope2"));
            }
        }
    }
}
