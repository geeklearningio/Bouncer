namespace GeekLearning.Bouncer.Tests
{
    using EntityFrameworkCore;
    using GeekLearning.Bouncer.EntityFrameworkCore.Data.Extensions;
    using Microsoft.EntityFrameworkCore;
    using Model.Client;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class AuthorizationTest : BouncerTestBase
    {
        private readonly Dictionary<string, EntityFrameworkCore.Data.Scope> testScopes = new Dictionary<string, EntityFrameworkCore.Data.Scope>();

        [Fact]
        public async Task AffectRoleOnScope_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                await bouncerFixture.AuthorizationsManager.CreateRoleAsync("role1", new string[] { "right1", "right2" });

                await bouncerFixture.AuthorizationsManager.CreateScopeAsync("scope1", "Scope 1");
                await bouncerFixture.AuthorizationsManager.CreateScopeAsync("scope2", "Scope 2");

                await bouncerFixture.AuthorizationsManager
                                          .AffectRoleToPrincipalOnScopeAsync(
                                               "role1",
                                               bouncerFixture.Context.CurrentUserId,
                                               "scope1");

                await bouncerFixture.Context.SaveChangesAsync();

                await bouncerFixture.AuthorizationsManager
                                           .AffectRoleToPrincipalOnScopeAsync(
                                                "role1",
                                                bouncerFixture.Context.CurrentUserId,
                                                "scope1");
                await bouncerFixture.AuthorizationsManager
                                           .UnaffectRoleFromPrincipalOnScopeAsync(
                                                "role1",
                                                bouncerFixture.Context.CurrentUserId,
                                                "scope1");
                await bouncerFixture.AuthorizationsManager
                                           .AffectRoleToPrincipalOnScopeAsync(
                                                "role1",
                                                bouncerFixture.Context.CurrentUserId,
                                                "scope1");
                await bouncerFixture.AuthorizationsManager
                                           .AffectRoleToPrincipalOnScopeAsync(
                                                "role1",
                                                bouncerFixture.Context.CurrentUserId,
                                                "scope2");

                bouncerFixture.Context.SaveChanges();

                var authorizations = bouncerFixture.Context.Authorizations
                                                                  .Include(a => a.Scope)
                                                                  .Include(a => a.Role)
                                                                  .Where(a => a.PrincipalId == bouncerFixture.Context.CurrentUserId)
                                                                  .ToList();

                Assert.Contains(authorizations, a => a.Role.Name == "role1");
                Assert.Contains(authorizations, s => s.Scope.Name == "scope1");
                Assert.Contains(authorizations, s => s.Scope.Name == "scope2");
            }
        }

        [Fact]
        public async Task UnaffectRoleFromPrincipalOnScope_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                // Test removing non existing authorization
                await bouncerFixture.AuthorizationsManager
                                           .UnaffectRoleFromPrincipalOnScopeAsync(
                                                "role1",
                                                bouncerFixture.Context.CurrentUserId,
                                                "scope1");

                await bouncerFixture.AuthorizationsManager.CreateRoleAsync("role1", new string[] { "right1", "right2" });

                await bouncerFixture.AuthorizationsManager.CreateScopeAsync("scope1", "Scope 1");

                await bouncerFixture.Context.SaveChangesAsync();

                // Test removing local existing authorization
                await bouncerFixture.AuthorizationsManager
                                           .AffectRoleToPrincipalOnScopeAsync(
                                                "role1",
                                                bouncerFixture.Context.CurrentUserId,
                                                "scope1");
                await bouncerFixture.AuthorizationsManager
                                           .UnaffectRoleFromPrincipalOnScopeAsync(
                                                "role1",
                                                bouncerFixture.Context.CurrentUserId,
                                                "scope1");

                await bouncerFixture.Context.SaveChangesAsync();

                Assert.Null(bouncerFixture.Context
                                                 .Authorizations
                                                 .FirstOrDefault(a => a.PrincipalId == bouncerFixture.Context.CurrentUserId));

                // Test persisted authorization
                await bouncerFixture.AuthorizationsManager
                                           .AffectRoleToPrincipalOnScopeAsync(
                                                "role1",
                                                bouncerFixture.Context.CurrentUserId,
                                                "scope1");

                await bouncerFixture.Context.SaveChangesAsync();

                await bouncerFixture.AuthorizationsManager
                                           .UnaffectRoleFromPrincipalOnScopeAsync(
                                                "role1",
                                                bouncerFixture.Context.CurrentUserId,
                                                "scope1");

                await bouncerFixture.Context.SaveChangesAsync();

                Assert.Null(bouncerFixture.Context
                                                 .Authorizations
                                                 .FirstOrDefault(a => a.PrincipalId == bouncerFixture.Context.CurrentUserId));
            }
        }

        [Fact]
        public async Task UnaffectRolesFromGroup_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                await bouncerFixture.AuthorizationsManager.CreateRoleAsync("role1", new string[] { "right1", "right2" });
                await bouncerFixture.AuthorizationsManager.CreateRoleAsync("role2", new string[] { "right2" });

                await bouncerFixture.AuthorizationsManager.CreateScopeAsync("scope1", "Scope 1");

                await bouncerFixture.AuthorizationsManager.CreateGroupAsync("group2", "group1");

                await bouncerFixture.AuthorizationsManager.AffectRoleToGroupOnScopeAsync("role1", "group1", "scope1");
                await bouncerFixture.AuthorizationsManager.AffectRoleToGroupOnScopeAsync("role2", "group1", "scope1");

                await bouncerFixture.Context.SaveChangesAsync();
                
                await bouncerFixture.AuthorizationsManager.UnaffectRolesFromGroupAsync("group1");

                await bouncerFixture.Context.SaveChangesAsync();

                Assert.Null(bouncerFixture.Context
                                                 .Authorizations
                                                 .Join(bouncerFixture.Context.Groups, a => a.PrincipalId, g => g.Id, (a, g) => g)
                                                 .FirstOrDefault(a => a.Name == "group1"));
            }
        }

        [Fact]
        public void HasXRightsOnScope_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                PrincipalRights rightsResult = new PrincipalRights(
                    bouncerFixture.Context.CurrentUserId,
                    "Scope1",
                    new List<ScopeRights>
                    {
                        new ScopeRights(
                            bouncerFixture.Context.CurrentUserId,
                            "Scope1",
                            new List<Right>
                            {
                                new Right(bouncerFixture.Context.CurrentUserId, "Scope1", "right1", true, bouncerFixture.Context.CurrentUserId),
                                new Right(bouncerFixture.Context.CurrentUserId, "Scope1", "right2", true, bouncerFixture.Context.CurrentUserId),
                            },
                            new List<Right>
                            {
                                new Right(bouncerFixture.Context.CurrentUserId, "Scope1", "right1", false, bouncerFixture.Context.CurrentUserId),
                                new Right(bouncerFixture.Context.CurrentUserId, "Scope1", "right2", false, bouncerFixture.Context.CurrentUserId),
                                new Right(bouncerFixture.Context.CurrentUserId, "Scope1", "right3", false, bouncerFixture.Context.CurrentUserId),
                                new Right(bouncerFixture.Context.CurrentUserId, "Scope1", "right4", false, bouncerFixture.Context.CurrentUserId),
                            }),
                        new ScopeRights(
                            bouncerFixture.Context.CurrentUserId,
                            "Scope2",
                            new List<Right>(),
                            new List<Right>
                            {
                                new Right(bouncerFixture.Context.CurrentUserId, "Scope2", "right4", false, bouncerFixture.Context.CurrentUserId),
                            }),
                        new ScopeRights(
                            bouncerFixture.Context.CurrentUserId,
                            "Scope1_Child1",
                            new List<Right>
                            {
                                new Right(bouncerFixture.Context.CurrentUserId, "Scope1_Child1", "right1", false, bouncerFixture.Context.CurrentUserId),
                                new Right(bouncerFixture.Context.CurrentUserId, "Scope1_Child1", "right2", false, bouncerFixture.Context.CurrentUserId),
                                new Right(bouncerFixture.Context.CurrentUserId, "Scope1_Child1", "right3", true, bouncerFixture.Context.CurrentUserId),
                            },
                            new List<Right>()),
                        new ScopeRights(
                            bouncerFixture.Context.CurrentUserId,
                            "Scope2_Child1",
                            new List<Right>
                            {
                                new Right(bouncerFixture.Context.CurrentUserId, "Scope2_Child1", "right4", true, bouncerFixture.Context.CurrentUserId),
                            },
                            new List<Right>()),
                        new ScopeRights(
                            bouncerFixture.Context.CurrentUserId,
                            "Scope1_Child2",
                            new List<Right>
                            {
                                new Right(bouncerFixture.Context.CurrentUserId, "Scope1_Child2", "right1", false, bouncerFixture.Context.CurrentUserId),
                                new Right(bouncerFixture.Context.CurrentUserId, "Scope1_Child2", "right2", false, bouncerFixture.Context.CurrentUserId),
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
            using (var bouncerFixture = new BouncerFixture())
            {
                await this.InitAuthorizationsAsync(bouncerFixture.Context, BouncerTarget.ChildGroup);

                var r = await bouncerFixture.AuthorizationsClient.GetRightsAsync("A");
                Assert.False(r.HasAnyRightUnderScope("A"));

                r = await bouncerFixture.AuthorizationsClient.GetRightsAsync("A", withChildren: true);
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
            using (var bouncerFixture = new BouncerFixture())
            {
                await this.InitAuthorizationsAsync(bouncerFixture.Context, BouncerTarget.ChildGroup);

                var r = await bouncerFixture.AuthorizationsClient.GetRightsAsync("G");
                Assert.False(r.HasAnyRightUnderScope("G"));

                r = await bouncerFixture.AuthorizationsClient.GetRightsAsync("G", withChildren: true);
                Assert.True(r.HasRightOnScope("right5", "R"));
            }
        }

        [Fact]
        public async Task GetRights_OfSpecificGroup_OnSpecificScope_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                await this.InitAuthorizationsAsync(bouncerFixture.Context, BouncerTarget.ChildGroup);

                var r = await bouncerFixture.AuthorizationsClient.GetRightsAsync("Q");

                Assert.True(r.HasRightOnScope("right5", "Q"));
            }
        }

        [Fact]
        public async Task GetRights_OfParentGroup_OnSpecificScope_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                await this.InitAuthorizationsAsync(bouncerFixture.Context, BouncerTarget.ParentGroup);
                
                var r = await bouncerFixture.AuthorizationsClient.GetRightsAsync("Q");

                Assert.True(r.HasRightOnScope("right5", "Q"));
            }
        }

        [Fact]
        public async Task GetRights_OnParentScope_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                await InitAuthorizationsAsync(bouncerFixture.Context);

                var r = await bouncerFixture.AuthorizationsClient.GetRightsAsync("M");

                Assert.True(r.HasRightOnScope("right6", "M"));
                Assert.False(r.HasExplicitRightOnScope("right6", "M"));
            }
        }

        [Fact]
        public async Task GetRights_Explicit_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                await InitAuthorizationsAsync(bouncerFixture.Context);

                var r = await bouncerFixture.AuthorizationsClient.GetRightsAsync("B");

                Assert.True(r.HasRightOnScope("right6", "B"));
                Assert.True(r.HasAnyRightUnderScope("B"));
                Assert.True(r.HasExplicitRightOnScope("right6", "B"));
                Assert.True(r.HasAnyExplicitRightOnScope("B"));
            }
        }

        [Fact]
        public async Task GetRights_OnSpecificScope_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                await InitAuthorizationsAsync(bouncerFixture.Context);

                var r = await bouncerFixture.AuthorizationsClient.GetRightsAsync("Q");

                Assert.True(r.HasRightOnScope("right5", "Q"));
                Assert.True(r.HasAnyRightUnderScope("Q"));
                Assert.False(r.HasExplicitRightOnScope("right5", "Q"));
            }
        }

        [Fact]
        public async Task GetRights_OnMultipleScopes_OneLevel_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                await InitAuthorizationsAsync(bouncerFixture.Context);

                var r = await bouncerFixture.AuthorizationsClient.GetRightsAsync("K");

                Assert.True(r.HasRightOnScope("right1", "K"));
                Assert.True(r.HasRightOnScope("right2", "K"));
                Assert.True(r.HasRightOnScope("right3", "K"));
                Assert.False(r.HasRightOnScope("right4", "K"));
            }
        }
        
        [Fact]
        public async Task GetRights_OnMultipleScopes_MultiLevel_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                await InitAuthorizationsAsync(bouncerFixture.Context);

                var r = await bouncerFixture.AuthorizationsClient.GetRightsAsync("P");

                Assert.True(r.HasRightOnScope("right1", "P"));
                Assert.True(r.HasRightOnScope("right2", "P"));
                Assert.True(r.HasRightOnScope("right3", "P"));
                Assert.False(r.HasRightOnScope("right4", "P"));
            }
        }

        [Fact]
        public async Task GetRights_OnMultipleScopes_MultiLevels_MultiBranches_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                await InitAuthorizationsAsync(bouncerFixture.Context);

                var r = await bouncerFixture.AuthorizationsClient.GetRightsAsync("U");

                Assert.True(r.HasRightOnScope("right1", "U"));
                Assert.True(r.HasRightOnScope("right2", "U"));
                Assert.True(r.HasRightOnScope("right3", "U"));
                Assert.True(r.HasRightOnScope("right3", "U"));
                Assert.False(r.HasRightOnScope("right4", "U"));
            }
        }

        [Fact]
        public async Task GetRightsOnScopeAfterReset_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                await bouncerFixture.AuthorizationsManager.CreateRoleAsync("role1", new string[] { "right1", "right2" });

                await bouncerFixture.AuthorizationsManager.CreateScopeAsync("scope2", "Scope 2", "scope1");

                await bouncerFixture.AuthorizationsManager.CreateGroupAsync("group2", "group1");

                await bouncerFixture.AuthorizationsManager.AddPrincipalToGroupAsync(bouncerFixture.Context.CurrentUserId, "group2");

                await bouncerFixture.Context.SaveChangesAsync();

                var group = await bouncerFixture.Context.Groups.FirstAsync(g => g.Name == "group1");
                await bouncerFixture.AuthorizationsManager.AffectRoleToPrincipalOnScopeAsync("role1", group.Id, "scope1");

                await bouncerFixture.Context.SaveChangesAsync();

                var r = await bouncerFixture.AuthorizationsClient.GetRightsAsync("scope1", withChildren: true);

                Assert.True(r.HasRightOnScope("right1", "scope2"));

                await bouncerFixture.AuthorizationsManager.UnaffectRoleFromPrincipalOnScopeAsync("role1", group.Id, "scope1");

                await bouncerFixture.Context.SaveChangesAsync();

                bouncerFixture.AuthorizationsClient.Reset();

                r = await bouncerFixture.AuthorizationsClient.GetRightsAsync("scope1", withChildren: true);

                Assert.False(r.HasRightOnScope("right1", "scope2"));

            }
        }

        //------> Scopes
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
        //
        //------> Authorizations
        // E | Role1: [ Right1, Right2 ]
        // F | Role2: [ Right3 ]
        // C | Role3: [ Right4 ]
        // L | Role4: [ Right5 ]
        // B | Role5: [ Right6 ]
        private void CreateTestScopeTree(BouncerTestContext context)
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

        private async Task InitAuthorizationsAsync(BouncerTestContext context, BouncerTarget authorizationsTarget = BouncerTarget.CurrentUser)
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
            if (authorizationsTarget != BouncerTarget.CurrentUser)
            {
                var groupParent = this.CreateGroup(context, "groupParent");
                var groupChild = this.CreateGroup(context, "groupChild");
                this.AddPrincipalToGroup(context, groupChild.Id, groupParent);
                this.AddPrincipalToGroup(context, context.CurrentUserId, groupChild);

                principalId = authorizationsTarget == BouncerTarget.ChildGroup ? groupChild.Id : groupParent.Id;
            }

            this.CreateAuthorization(context, principalId, role1, testScopes["E"]);
            this.CreateAuthorization(context, principalId, role2, testScopes["F"]);
            this.CreateAuthorization(context, principalId, role3, testScopes["C"]);
            this.CreateAuthorization(context, principalId, role4, testScopes["L"]);
            this.CreateAuthorization(context, principalId, role5, testScopes["B"]);

            await context.SaveChangesAsync();
        }
    }
}
