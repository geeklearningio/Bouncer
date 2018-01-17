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
        public void HasXRightsOnScope_ShouldBeOk()
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
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1", "right1", true, authorizationsFixture.Context.CurrentUserId),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1", "right2", true, authorizationsFixture.Context.CurrentUserId),
                            },
                            new List<Right>
                            {
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1", "right1", false, authorizationsFixture.Context.CurrentUserId),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1", "right2", false, authorizationsFixture.Context.CurrentUserId),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1", "right3", false, authorizationsFixture.Context.CurrentUserId),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1", "right4", false, authorizationsFixture.Context.CurrentUserId),
                            }),
                        new ScopeRights(
                            authorizationsFixture.Context.CurrentUserId,
                            "Scope2",
                            new List<Right>(),
                            new List<Right>
                            {
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope2", "right4", false, authorizationsFixture.Context.CurrentUserId),
                            }),
                        new ScopeRights(
                            authorizationsFixture.Context.CurrentUserId,
                            "Scope1_Child1",
                            new List<Right>
                            {
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1_Child1", "right1", false, authorizationsFixture.Context.CurrentUserId),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1_Child1", "right2", false, authorizationsFixture.Context.CurrentUserId),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1_Child1", "right3", true, authorizationsFixture.Context.CurrentUserId),
                            },
                            new List<Right>()),
                        new ScopeRights(
                            authorizationsFixture.Context.CurrentUserId,
                            "Scope2_Child1",
                            new List<Right>
                            {
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope2_Child1", "right4", true, authorizationsFixture.Context.CurrentUserId),
                            },
                            new List<Right>()),
                        new ScopeRights(
                            authorizationsFixture.Context.CurrentUserId,
                            "Scope1_Child2",
                            new List<Right>
                            {
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1_Child2", "right1", false, authorizationsFixture.Context.CurrentUserId),
                                new Right(authorizationsFixture.Context.CurrentUserId, "Scope1_Child2", "right2", false, authorizationsFixture.Context.CurrentUserId),
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
        }

        [Fact]
        public async Task GetRights_WithChildren_MultiBranches_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await this.InitAuthorizationsAsync(authorizationsFixture.Context, AuthorizationsTarget.ChildGroup);

                var r = await authorizationsFixture.AuthorizationsClient.GetRightsAsync("A");
                Assert.False(r.HasAnyRightUnderScope("A"));

                r = await authorizationsFixture.AuthorizationsClient.GetRightsAsync("A", withChildren: true);
                Assert.True(r.HasRightOnScope("right1", "U"));
                Assert.True(r.HasRightOnScope("right2", "U"));
                Assert.True(r.HasRightOnScope("right3", "U"));
                Assert.True(r.HasRightOnScope("right5", "U"));
                Assert.True(r.HasRightOnScope("right5", "L"));
                Assert.True(r.HasRightOnScope("right6", "M"));
                Assert.True(r.HasRightOnScope("right6", "T"));
                Assert.False(r.HasRightOnScope("right4", "U"));
                Assert.False(r.HasRightOnScope("right6", "U"));
            }
        }

        [Fact]
        public async Task GetRights_WithChildren_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await this.InitAuthorizationsAsync(authorizationsFixture.Context, AuthorizationsTarget.ChildGroup);

                var r = await authorizationsFixture.AuthorizationsClient.GetRightsAsync("G");
                Assert.False(r.HasAnyRightUnderScope("G"));

                r = await authorizationsFixture.AuthorizationsClient.GetRightsAsync("G", withChildren: true);
                Assert.True(r.HasRightOnScope("right5", "R"));
            }
        }

        [Fact]
        public async Task GetRights_OfSpecificGroup_OnSpecificScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await this.InitAuthorizationsAsync(authorizationsFixture.Context, AuthorizationsTarget.ChildGroup);

                var r = await authorizationsFixture.AuthorizationsClient.GetRightsAsync("Q");

                Assert.True(r.HasRightOnScope("right5", "Q"));
            }
        }

        [Fact]
        public async Task GetRights_OfParentGroup_OnSpecificScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await this.InitAuthorizationsAsync(authorizationsFixture.Context, AuthorizationsTarget.ParentGroup);
                
                var r = await authorizationsFixture.AuthorizationsClient.GetRightsAsync("Q");

                Assert.True(r.HasRightOnScope("right5", "Q"));
            }
        }

        [Fact]
        public async Task GetRights_OnParentScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await InitAuthorizationsAsync(authorizationsFixture.Context);

                var r = await authorizationsFixture.AuthorizationsClient.GetRightsAsync("M");

                Assert.True(r.HasRightOnScope("right6", "M"));
            }
        }

        [Fact]
        public async Task GetRights_OnSpecificScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await InitAuthorizationsAsync(authorizationsFixture.Context);

                var r = await authorizationsFixture.AuthorizationsClient.GetRightsAsync("Q");

                Assert.True(r.HasRightOnScope("right5", "Q"));
            }
        }

        [Fact]
        public async Task GetRights_OnMultipleScopes_OneLevel_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await InitAuthorizationsAsync(authorizationsFixture.Context);

                var r = await authorizationsFixture.AuthorizationsClient.GetRightsAsync("K");

                Assert.True(r.HasRightOnScope("right1", "K"));
                Assert.True(r.HasRightOnScope("right2", "K"));
                Assert.True(r.HasRightOnScope("right3", "K"));
                Assert.False(r.HasRightOnScope("right4", "K"));
            }
        }
        
        [Fact]
        public async Task GetRights_OnMultipleScopes_MultiLevel_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await InitAuthorizationsAsync(authorizationsFixture.Context);

                var r = await authorizationsFixture.AuthorizationsClient.GetRightsAsync("P");

                Assert.True(r.HasRightOnScope("right1", "P"));
                Assert.True(r.HasRightOnScope("right2", "P"));
                Assert.True(r.HasRightOnScope("right3", "P"));
                Assert.False(r.HasRightOnScope("right4", "P"));
            }
        }

        [Fact]
        public async Task GetRights_OnMultipleScopes_MultiLevels_MultiBranches_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await InitAuthorizationsAsync(authorizationsFixture.Context);

                var r = await authorizationsFixture.AuthorizationsClient.GetRightsAsync("U");

                Assert.True(r.HasRightOnScope("right1", "U"));
                Assert.True(r.HasRightOnScope("right2", "U"));
                Assert.True(r.HasRightOnScope("right3", "U"));
                Assert.True(r.HasRightOnScope("right3", "U"));
                Assert.False(r.HasRightOnScope("right4", "U"));
            }
        }

        // This is a representation of the test scope tree
        //
        //                           +-+                                +-+
        //                           |A|                                |B|
        //                           +-+                                +-+
        //                            ^                                  ^
        //                            |                                  |
        //       +-----+-----+-----+--+--+-----+-----+-----+             |
        //       |     |     |     |     |     |     |     |             |
        //      +-+   +-+   +-+   +-+   +-+   +-+   +-+   +-+           +-+
        //      |C|   |D|   |E|   |F|   |G|   |H|   |I|   |J|           |V|
        //      +-+   +-+   +-+   +-+   +-+   +-+   +-+   +-+           +-+
        //       ^     ^     ^     ^     ^     ^     ^     ^             ^
        //       |     |     |     |     |     |     |     |             |
        //       +--+--+     +--+--+     +--+--+  +--+--+--+             |        
        //          |           |           |     |     |                |
        //          |           |           |     |     +----------------+
        //          |           |           |     |     |        
        //          |          +-+         +-+    |    +-+
        //          |          |K|         |L|    |    |M|
        //          |          +-+         +-+    |    +-+
        //          |           ^           ^     |     ^
        //          |           |           |     |     |
        //          |         +-+-+       +-+-+---+   +-+-+
        //          |         |   |       |   |       |   |        
        //         +-+       +-+ +-+     +-+ +-+     +-+ +-+
        //         |N|       |O| |P|     |Q| |R|     |S| |T|
        //         +-+       +-+ +-+     +-+ +-+     +-+ +-+
        //                        ^       ^
        //                        |       |
        //                        +---+---+
        //                            |
        //                           +-+
        //                           |U|
        //                           +-+
        private void CreateTestScopeTree(AuthorizationsTestContext context)
        {
            this.testScopes["A"] = this.CreateScope(context, "A");
            this.testScopes["B"] = this.CreateScope(context, "B");
            this.testScopes["C"] = this.CreateScope(context, "C", "A");
            this.testScopes["D"] = this.CreateScope(context, "D", "A");
            this.testScopes["E"] = this.CreateScope(context, "E", "A");
            this.testScopes["F"] = this.CreateScope(context, "F", "A");
            this.testScopes["G"] = this.CreateScope(context, "G", "A");
            this.testScopes["H"] = this.CreateScope(context, "H", "A");
            this.testScopes["I"] = this.CreateScope(context, "I", "A");
            this.testScopes["J"] = this.CreateScope(context, "J", "A");
            this.testScopes["V"] = this.CreateScope(context, "V", "B");
            this.testScopes["N"] = this.CreateScope(context, "N", "C", "D");
            this.testScopes["K"] = this.CreateScope(context, "K", "E", "F");
            this.testScopes["L"] = this.CreateScope(context, "L", "G", "H");
            this.testScopes["M"] = this.CreateScope(context, "M", "I", "J", "V");
            this.testScopes["O"] = this.CreateScope(context, "O", "K");
            this.testScopes["P"] = this.CreateScope(context, "P", "K");
            this.testScopes["Q"] = this.CreateScope(context, "Q", "L", "I", "J");
            this.testScopes["R"] = this.CreateScope(context, "R", "L", "I", "J");
            this.testScopes["S"] = this.CreateScope(context, "S", "M");
            this.testScopes["R"] = this.CreateScope(context, "T", "M");
            this.testScopes["U"] = this.CreateScope(context, "U", "P", "Q");
        }

        private async Task InitAuthorizationsAsync(AuthorizationsTestContext context, AuthorizationsTarget authorizationsTarget = AuthorizationsTarget.CurrentUser)
        {
            var right1 = this.CreateRight(context, "right1");
            var right2 = this.CreateRight(context, "right2");
            var role1 = this.CreateRole(context, "role1");
            var right3 = this.CreateRight(context, "right3");
            var role2 = this.CreateRole(context, "role2");
            var right4 = this.CreateRight(context, "right4");
            var role3 = this.CreateRole(context, "role3");
            var right5 = this.CreateRight(context, "right5");
            var role4 = this.CreateRole(context, "role4");
            var right6 = this.CreateRight(context, "right6");
            var role5 = this.CreateRole(context, "role5");
            this.AddRightToRole(context, right1, role1);
            this.AddRightToRole(context, right2, role1);
            this.AddRightToRole(context, right3, role2);
            this.AddRightToRole(context, right4, role3);
            this.AddRightToRole(context, right5, role4);
            this.AddRightToRole(context, right6, role5);

            this.CreateTestScopeTree(context);

            var principalId = context.CurrentUserId;
            if (authorizationsTarget != AuthorizationsTarget.CurrentUser)
            {
                var groupParent = this.CreateGroup(context, "groupParent");
                var groupChild = this.CreateGroup(context, "groupChild");
                this.AddPrincipalToGroup(context, groupChild.Id, groupParent);
                this.AddPrincipalToGroup(context, context.CurrentUserId, groupChild);

                principalId = authorizationsTarget == AuthorizationsTarget.ChildGroup ? groupChild.Id : groupParent.Id;
            }

            this.CreateAuthorization(context, principalId, role1, testScopes["E"]);
            this.CreateAuthorization(context, principalId, role2, testScopes["F"]);
            this.CreateAuthorization(context, principalId, role3, testScopes["C"]);
            this.CreateAuthorization(context, principalId, role4, testScopes["L"]);
            this.CreateAuthorization(context, principalId, role5, testScopes["B"]);

            await context.SaveChangesAsync();
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
