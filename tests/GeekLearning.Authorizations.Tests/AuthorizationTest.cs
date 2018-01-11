namespace GeekLearning.Authorizations.Tests
{
    using EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Model.Client;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class AuthorizationTest : AuthorizationsTestBase
    {
        private Dictionary<string, EntityFrameworkCore.Data.Scope> testScopes = new Dictionary<string, EntityFrameworkCore.Data.Scope>();

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

        [Fact]
        public async Task GetRights_OfSpecificGroup_OnSpecificScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                var right1 = this.CreateRight(authorizationsFixture.Context, "right1");
                var role1 = this.CreateRole(authorizationsFixture.Context, "role1");
                this.AddRightToRole(authorizationsFixture.Context, right1, role1);

                var groupParent = this.CreateGroup(authorizationsFixture.Context, "groupParent");
                var groupChild = this.CreateGroup(authorizationsFixture.Context, "groupChild");
                this.AddPrincipalToGroup(authorizationsFixture.Context, groupChild.Id, groupParent);
                this.AddPrincipalToGroup(authorizationsFixture.Context, authorizationsFixture.Context.CurrentUserId, groupChild);

                this.CreateTestScopeTree(authorizationsFixture);

                this.CreateAuthorization(authorizationsFixture.Context, groupChild.Id, role1, testScopes["R"]);

                await authorizationsFixture.Context.SaveChangesAsync();

                var r = await authorizationsFixture.AuthorizationsClient.GetRightsAsync("R");

                Assert.True(r.HasRightOnScope("right1", "R"));
            }
        }

        [Fact]
        public async Task GetRights_OnParentScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                var right1 = this.CreateRight(authorizationsFixture.Context, "right1");
                var role1 = this.CreateRole(authorizationsFixture.Context, "role1");
                this.AddRightToRole(authorizationsFixture.Context, right1, role1);

                this.CreateTestScopeTree(authorizationsFixture);

                this.CreateAuthorization(authorizationsFixture.Context, authorizationsFixture.Context.CurrentUserId, role1, testScopes["E"]);

                await authorizationsFixture.Context.SaveChangesAsync();

                var r = await authorizationsFixture.AuthorizationsClient.GetRightsAsync("R");

                Assert.True(r.HasRightOnScope("right1", "R"));
            }
        }

        [Fact]
        public async Task GetRights_OnSpecificScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                var right1 = this.CreateRight(authorizationsFixture.Context, "right1");                
                var role1 = this.CreateRole(authorizationsFixture.Context, "role1");
                this.AddRightToRole(authorizationsFixture.Context, right1, role1);
               
                this.CreateTestScopeTree(authorizationsFixture);

                this.CreateAuthorization(authorizationsFixture.Context, authorizationsFixture.Context.CurrentUserId, role1, testScopes["R"]);                

                await authorizationsFixture.Context.SaveChangesAsync();

                var r = await authorizationsFixture.AuthorizationsClient.GetRightsAsync("R");

                Assert.True(r.HasRightOnScope("right1", "R"));
            }
        }

        //                  +-+
        //                  |A|
        //                  +-+
        //                   |
        //       +-----+-----+-----+-----+
        //       |     |     |     |     |
        //       v     v     v     v     v
        //      +-+   +-+   +-+   +-+   +-+
        //      |B|   |C|   |D|   |E|   |F|
        //      +-+   +-+   +-+   +-+   +-+
        //       |     |     |     |     |
        //       +--+--+--+--+--+--+--+--+
        //          |     |     |     |
        //          v     v     v     v
        //         +-+   +-+   +-+   +-+
        //         |G|   |H|   |I|   |J|
        //         +-+   +-+   +-+   +-+
        //          |     |     |     |
        //  +---+---+ +---+     +---+ +---+---+
        //  |   |     |   |     |   |     |   |
        //  v   v     v   v     v   v     v   v
        // +-+ +-+   +-+ +-+   +-+ +-+   +-+ +-+
        // |K| |L|   |M| |N|   |O| |P|   |Q| |R|
        // +-+ +-+   +-+ +-+   +-+ +-+   +-+ +-+
        private void CreateTestScopeTree(AuthorizationsFixture authorizationsFixture)
        {
            this.testScopes["A"] = this.CreateScope(authorizationsFixture.Context, "A");
            this.testScopes["B"] = this.CreateScope(authorizationsFixture.Context, "B", "A");
            this.testScopes["C"] = this.CreateScope(authorizationsFixture.Context, "C", "A");
            this.testScopes["D"] = this.CreateScope(authorizationsFixture.Context, "D", "A");
            this.testScopes["E"] = this.CreateScope(authorizationsFixture.Context, "E", "A");
            this.testScopes["F"] = this.CreateScope(authorizationsFixture.Context, "F", "A");
            this.testScopes["G"] = this.CreateScope(authorizationsFixture.Context, "G", "B", "C");
            this.testScopes["H"] = this.CreateScope(authorizationsFixture.Context, "H", "C", "D");
            this.testScopes["I"] = this.CreateScope(authorizationsFixture.Context, "I", "D", "E");
            this.testScopes["J"] = this.CreateScope(authorizationsFixture.Context, "J", "E", "F");
            this.testScopes["K"] = this.CreateScope(authorizationsFixture.Context, "K", "G");
            this.testScopes["L"] = this.CreateScope(authorizationsFixture.Context, "L", "G");
            this.testScopes["M"] = this.CreateScope(authorizationsFixture.Context, "M", "H");
            this.testScopes["N"] = this.CreateScope(authorizationsFixture.Context, "N", "H");
            this.testScopes["O"] = this.CreateScope(authorizationsFixture.Context, "O", "I");
            this.testScopes["P"] = this.CreateScope(authorizationsFixture.Context, "P", "I");
            this.testScopes["Q"] = this.CreateScope(authorizationsFixture.Context, "Q", "J");
            this.testScopes["R"] = this.CreateScope(authorizationsFixture.Context, "R", "J");
        }
    }
}
