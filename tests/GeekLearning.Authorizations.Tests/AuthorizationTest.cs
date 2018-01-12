namespace GeekLearning.Authorizations.Tests
{
    using EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Model.Client;
    using System;
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
                await authorizationsFixture.AuthorizationsManager.CreateRoleAsync("role1", new string[] { "right1", "right2" });

                await authorizationsFixture.AuthorizationsManager.CreateScopeAsync("scope1", "Scope 1");
                await authorizationsFixture.AuthorizationsManager.CreateScopeAsync("scope2", "Scope 2");

                await authorizationsFixture.AuthorizationsManager
                                          .AffectRoleToPrincipalOnScopeAsync(
                                               "role1",
                                               authorizationsFixture.Context.CurrentUserId,
                                               "scope1");

                await authorizationsFixture.Context.SaveChangesAsync();

                await authorizationsFixture.AuthorizationsManager
                                           .AffectRoleToPrincipalOnScopeAsync(
                                                "role1",
                                                authorizationsFixture.Context.CurrentUserId,
                                                "scope1");
                await authorizationsFixture.AuthorizationsManager
                                           .UnaffectRoleFromPrincipalOnScopeAsync(
                                                "role1",
                                                authorizationsFixture.Context.CurrentUserId,
                                                "scope1");
                await authorizationsFixture.AuthorizationsManager
                                           .AffectRoleToPrincipalOnScopeAsync(
                                                "role1",
                                                authorizationsFixture.Context.CurrentUserId,
                                                "scope1");
                await authorizationsFixture.AuthorizationsManager
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
        public async Task UnaffectRoleFromPrincipalOnScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                // Test removing non existing authorization
                await authorizationsFixture.AuthorizationsManager
                                           .UnaffectRoleFromPrincipalOnScopeAsync(
                                                "role1",
                                                authorizationsFixture.Context.CurrentUserId,
                                                "scope1");

                await authorizationsFixture.AuthorizationsManager.CreateRoleAsync("role1", new string[] { "right1", "right2" });

                await authorizationsFixture.AuthorizationsManager.CreateScopeAsync("scope1", "Scope 1");

                await authorizationsFixture.Context.SaveChangesAsync();

                // Test removing local existing authorization
                await authorizationsFixture.AuthorizationsManager
                                           .AffectRoleToPrincipalOnScopeAsync(
                                                "role1",
                                                authorizationsFixture.Context.CurrentUserId,
                                                "scope1");
                await authorizationsFixture.AuthorizationsManager
                                           .UnaffectRoleFromPrincipalOnScopeAsync(
                                                "role1",
                                                authorizationsFixture.Context.CurrentUserId,
                                                "scope1");

                await authorizationsFixture.Context.SaveChangesAsync();

                Assert.Null(authorizationsFixture.Context
                                                 .Authorizations()
                                                 .FirstOrDefault(a => a.PrincipalId == authorizationsFixture.Context.CurrentUserId));

                // Test persisted authorization
                await authorizationsFixture.AuthorizationsManager
                                           .AffectRoleToPrincipalOnScopeAsync(
                                                "role1",
                                                authorizationsFixture.Context.CurrentUserId,
                                                "scope1");

                await authorizationsFixture.Context.SaveChangesAsync();

                await authorizationsFixture.AuthorizationsManager
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
        public async Task UnaffectRolesFromGroup_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await authorizationsFixture.AuthorizationsManager.CreateRoleAsync("role1", new string[] { "right1", "right2" });
                await authorizationsFixture.AuthorizationsManager.CreateRoleAsync("role2", new string[] { "right2" });

                await authorizationsFixture.AuthorizationsManager.CreateScopeAsync("scope1", "Scope 1");

                await authorizationsFixture.AuthorizationsManager.CreateGroupAsync("group2", "group1");

                await authorizationsFixture.AuthorizationsManager.AffectRoleToGroupOnScopeAsync("role1", "group1", "scope1");
                await authorizationsFixture.AuthorizationsManager.AffectRoleToGroupOnScopeAsync("role2", "group1", "scope1");

                await authorizationsFixture.Context.SaveChangesAsync();
                
                await authorizationsFixture.AuthorizationsManager.UnaffectRolesFromGroupAsync("group1");

                await authorizationsFixture.Context.SaveChangesAsync();

                Assert.Null(authorizationsFixture.Context
                                                 .Authorizations()
                                                 .Join(authorizationsFixture.Context.Groups(), a => a.PrincipalId, g => g.Id, (a, g) => g)
                                                 .FirstOrDefault(a => a.Name == "group1"));
            }
        }

        [Fact]
        public Task HasXRightsOnScope_ShouldBeOk()
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
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1", "right1", true, Guid.NewGuid()),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1", "right2", true, Guid.NewGuid()),
                            },
                            new List<Right>
                            {
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1", "right1", false, Guid.NewGuid()),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1", "right2", false, Guid.NewGuid()),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1", "right3", false, Guid.NewGuid()),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1", "right4", false, Guid.NewGuid()),
                            }),
                        new ScopeRights(
                            authorizationsFixture.Context.CurrentUserId,
                            "Scope2",
                            new List<Right>(),
                            new List<Right>
                            {
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope2", "right4", false, Guid.NewGuid()),
                            }),
                        new ScopeRights(
                            authorizationsFixture.Context.CurrentUserId,
                            "Scope1_Child1",
                            new List<Right>
                            {
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1_Child1", "right1", false, Guid.NewGuid()),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1_Child1", "right2", false, Guid.NewGuid()),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1_Child1", "right3", true, Guid.NewGuid()),
                            },
                            new List<Right>()),
                        new ScopeRights(
                            authorizationsFixture.Context.CurrentUserId,
                            "Scope2_Child1",
                            new List<Right>
                            {
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope2_Child1", "right4", true, Guid.NewGuid()),
                            },
                            new List<Right>()),
                        new ScopeRights(
                            authorizationsFixture.Context.CurrentUserId,
                            "Scope1_Child2",
                            new List<Right>
                            {
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1_Child2", "right1", false, Guid.NewGuid()),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1_Child2", "right2", false, Guid.NewGuid()),
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

        [Fact]
        public async Task GetRightsOnScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await authorizationsFixture.AuthorizationsManager.CreateRoleAsync("role1", new string[] { "right1", "right2" });

                await authorizationsFixture.AuthorizationsManager.CreateScopeAsync("scope2", "Scope 2", "scope1");

                await authorizationsFixture.AuthorizationsManager.CreateGroupAsync("group2", "group1");

                await authorizationsFixture.AuthorizationsManager.AddPrincipalToGroupAsync(authorizationsFixture.Context.CurrentUserId, "group2");

                await authorizationsFixture.Context.SaveChangesAsync();

                var group = await authorizationsFixture.Context.Groups().FirstAsync(g => g.Name == "group1");
                await authorizationsFixture.AuthorizationsManager.AffectRoleToPrincipalOnScopeAsync("role1", group.Id, "scope1");

                await authorizationsFixture.Context.SaveChangesAsync();

                var r = await authorizationsFixture.AuthorizationsClient.GetRightsAsync("scope1", withChildren: true);

                Assert.True(r.HasRightOnScope("right1", "scope2"));
            }
        }

        [Fact]
        public async Task GetRightsOnScopeAfterReset_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await authorizationsFixture.AuthorizationsManager.CreateRoleAsync("role1", new string[] { "right1", "right2" });

                await authorizationsFixture.AuthorizationsManager.CreateScopeAsync("scope2", "Scope 2", "scope1");

                await authorizationsFixture.AuthorizationsManager.CreateGroupAsync("group2", "group1");

                await authorizationsFixture.AuthorizationsManager.AddPrincipalToGroupAsync(authorizationsFixture.Context.CurrentUserId, "group2");

                await authorizationsFixture.Context.SaveChangesAsync();

                var group = await authorizationsFixture.Context.Groups().FirstAsync(g => g.Name == "group1");
                await authorizationsFixture.AuthorizationsManager.AffectRoleToPrincipalOnScopeAsync("role1", group.Id, "scope1");

                await authorizationsFixture.Context.SaveChangesAsync();

                var r = await authorizationsFixture.AuthorizationsClient.GetRightsAsync("scope1", withChildren: true);

                Assert.True(r.HasRightOnScope("right1", "scope2"));

                await authorizationsFixture.AuthorizationsManager.UnaffectRoleFromPrincipalOnScopeAsync("role1", group.Id, "scope1");

                await authorizationsFixture.Context.SaveChangesAsync();

                authorizationsFixture.AuthorizationsClient.Reset();

                r = await authorizationsFixture.AuthorizationsClient.GetRightsAsync("scope1", withChildren: true);

                Assert.False(r.HasRightOnScope("right1", "scope2"));

            }
        }
    }
}
