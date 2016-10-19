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
                await authorizationsFixture.AuthorizationsProvisioningClient.CreateRoleAsync("role1", new string[] { "right1", "right2" });

                await authorizationsFixture.AuthorizationsProvisioningClient.CreateScopeAsync("scope1", "Scope 1");

                await authorizationsFixture.AuthorizationsProvisioningClient
                                          .AffectRoleToPrincipalOnScopeAsync(
                                               "role1",
                                               authorizationsFixture.Context.CurrentUserId,
                                               "scope1");

                await authorizationsFixture.Context.SaveChangesAsync();

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
                await authorizationsFixture.AuthorizationsProvisioningClient
                                           .AffectRoleToPrincipalOnScopeAsync(
                                                "role1",
                                                authorizationsFixture.Context.CurrentUserId,
                                                "scope1");

                authorizationsFixture.Context.SaveChanges();

                var authorization = authorizationsFixture.Context.Authorizations()
                                                                 .Include(a => a.Scope)
                                                                 .Include(a => a.Role)
                                                                 .SingleOrDefault(a => a.PrincipalId == authorizationsFixture.Context.CurrentUserId);

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
                // Test removing non existing authorization
                await authorizationsFixture.AuthorizationsProvisioningClient
                                           .UnaffectRoleFromPrincipalOnScopeAsync(
                                                "role1",
                                                authorizationsFixture.Context.CurrentUserId,
                                                "scope1");

                await authorizationsFixture.AuthorizationsProvisioningClient.CreateRoleAsync("role1", new string[] { "right1", "right2" });                

                await authorizationsFixture.AuthorizationsProvisioningClient.CreateScopeAsync("scope1", "Scope 1");                

                await authorizationsFixture.AuthorizationsProvisioningClient
                                           .AffectRoleToPrincipalOnScopeAsync(
                                                "role1",
                                                authorizationsFixture.Context.CurrentUserId,
                                                "scope1");

                await authorizationsFixture.Context.SaveChangesAsync();

                // Test removing local existing authorization
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

                await authorizationsFixture.Context.SaveChangesAsync();

                Assert.Null(authorizationsFixture.Context
                                                 .Authorizations()
                                                 .FirstOrDefault(a => a.PrincipalId == authorizationsFixture.Context.CurrentUserId));
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

                Assert.True(await authorizationsFixture.AuthorizationsClient.HasRightAsync("right3", "Scope1_Child1"));
            }
        }
    }
}
