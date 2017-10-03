namespace GeekLearning.Authorizations.Tests
{
    using EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Model;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class AuthorizationTest
    {
        [Fact]
        public async Task AffectRoleOnScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await authorizationsFixture.AuthorizationsProvisioningClient.CreateRoleAsync("role1", new string[] { "right1", "right2" });

                await authorizationsFixture.AuthorizationsProvisioningClient.CreateScopeAsync("scope1", "Scope 1");
                await authorizationsFixture.AuthorizationsProvisioningClient.CreateScopeAsync("scope2", "Scope 2");

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
                await authorizationsFixture.AuthorizationsProvisioningClient
                                           .AffectRoleToPrincipalOnScopeAsync(
                                                "role1",
                                                authorizationsFixture.Context.CurrentUserId,
                                                "scope2");

                authorizationsFixture.Context.SaveChanges();

                var authorizations = authorizationsFixture.Context.Authorizations()
                                                                  .Include(a => a.Scope)
                                                                  .Include(a => a.Role)
                                                                  .Where(a => a.PrincipalId == authorizationsFixture.Context.CurrentUserId)
                                                                  .ToList();

                Assert.True(authorizations.Any(a => a.Role.Name == "role1"));
                Assert.True(authorizations.Any(s => s.Scope.Name == "scope1"));
                Assert.True(authorizations.Any(s => s.Scope.Name == "scope2"));
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
        public Task GetRightsOnScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                PrincipalRights rightsResult = new PrincipalRights(
                    authorizationsFixture.Context.CurrentUserId,
                    "Scope1",
                    new List<ScopeRights>
                    {
                        new ScopeRights(
                            authorizationsFixture.Context.CurrentUserId,
                            "Scope1",
                            new List<Right>
                            {
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1", "right1", true),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1", "right2", true),
                            },
                            new List<Right>
                            {
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1", "right1", false),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1", "right2", false),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1", "right3", false),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1", "right4", false),
                            }),
                        new ScopeRights(
                            authorizationsFixture.Context.CurrentUserId,
                            "Scope2",
                            new List<Right>(),
                            new List<Right>
                            {
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope2", "right4", false),
                            }),
                        new ScopeRights(
                            authorizationsFixture.Context.CurrentUserId,
                            "Scope1_Child1",
                            new List<Right>
                            {
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1_Child1", "right1", false),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1_Child1", "right2", false),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1_Child1", "right3", true),
                            },
                            new List<Right>()),
                        new ScopeRights(
                            authorizationsFixture.Context.CurrentUserId,
                            "Scope2_Child1",
                            new List<Right>
                            {
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope2_Child1", "right4", true),
                            },
                            new List<Right>()),
                        new ScopeRights(
                            authorizationsFixture.Context.CurrentUserId,
                            "Scope1_Child2",
                            new List<Right>
                            {
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1_Child2", "right1", false),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1_Child2", "right2", false),
                            },
                            new List<Right>()),
                    });

                Assert.True(rightsResult.HasRightOnScope("right3", "Scope1_Child1"));
                Assert.True(rightsResult.HasRightOnScope("right1", "Scope1_Child1"));
                Assert.False(rightsResult.HasRightOnScope("right3", "Scope1"));
                Assert.True(rightsResult.HasAnyRightUnderScope("Scope1"));
                Assert.True(rightsResult.HasAnyRightUnderScope("Scope1_Child1"));
                Assert.True(rightsResult.HasAnyRightUnderScope("Scope2"));
                Assert.True(rightsResult.HasRightUnderScope("right3", "Scope1"));
                Assert.False(rightsResult.HasAnyExplicitRightOnScope("Scope1_Child2"));
                Assert.True(rightsResult.HasInheritedRightOnScope("right1", "Scope1_Child2"));
            }

            return Task.CompletedTask;
        }
    }
}
