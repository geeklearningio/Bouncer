namespace GeekLearning.Authorizations.Tests
{
    using EntityFrameworkCore;
    using EntityFrameworkCore.Data;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class GroupTests
    {
        [Fact]
        public async Task CreateGroup_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await authorizationsFixture.AuthorizationsManager.CreateGroupAsync("group2", parentGroupName: "group1");

                await authorizationsFixture.Context.SaveChangesAsync();

                var group = authorizationsFixture.Context
                    .Groups()
                    .FirstOrDefault(r => r.Name == "group2");
                Assert.NotNull(group);

                var membership = authorizationsFixture.Context.Memberships().FirstOrDefault(m => m.Group.Name == "group1");
                group = authorizationsFixture.Context.Groups().FirstOrDefault(g => g.Id == membership.PrincipalId);
                Assert.Equal("group2", group.Name);
            }
        }

        [Fact]
        public async Task AddPrincipalToGroup_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                var parentGroup = new Group { Name = "group1" };
                var childGroup = new Group { Id = Guid.NewGuid(), Name = "group2" };
                authorizationsFixture.Context.Groups().Add(parentGroup);
                authorizationsFixture.Context.Groups().Add(childGroup);
                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = parentGroup,
                    PrincipalId = childGroup.Id
                });

                var scope = new Scope { Name = "Scope1", Description = "Scope1" };
                authorizationsFixture.Context.Scopes().Add(scope);

                var right = new Right { Name = "Right1" };
                authorizationsFixture.Context.Rights().Add(right);

                var role = new Role { Name = "Role1" };
                authorizationsFixture.Context.Roles().Add(role);

                authorizationsFixture.Context.Set<RoleRight>().Add(new RoleRight { Role = role, Right = right });

                authorizationsFixture.Context.Authorizations().Add(new Authorization
                {
                    Scope = scope,
                    Role = role,
                    PrincipalId = childGroup.Id
                });

                await authorizationsFixture.Context.SaveChangesAsync();

                var group = authorizationsFixture.Context
                    .Groups()
                    .FirstOrDefault(r => r.Name == "group2");

                await authorizationsFixture.AuthorizationsManager
                    .AddPrincipalToGroupAsync(authorizationsFixture.Context.CurrentUserId, "group2");

                await authorizationsFixture.Context.SaveChangesAsync();

                var membership = authorizationsFixture.Context.Memberships().FirstOrDefault(m => m.PrincipalId == authorizationsFixture.Context.CurrentUserId);
                Assert.NotNull(membership);

                await authorizationsFixture.AuthorizationsEventQueuer.CommitAsync();

                var rights = JsonConvert.DeserializeObject<Model.Client.PrincipalRights>(
                    authorizationsFixture.AuthorizationsImpactClient.UserDenormalizedRights[authorizationsFixture.Context.CurrentUserId]["Scope1"]);
                Assert.True(rights.HasRightOnScope("Right1", "Scope1"));
            }
        }

        [Fact]
        public async Task GetGroupParentLink_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                List<Group> expectedGroups = new List<Group>();
                for (int i = 0; i < 10; i++)
                {
                    var group = new Group { Id = Guid.NewGuid(), Name = "group" + i };
                    expectedGroups.Add(group);
                    authorizationsFixture.Context.Groups().Add(group);
                }

                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = expectedGroups[0],
                    PrincipalId = expectedGroups[1].Id
                });
                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = expectedGroups[0],
                    PrincipalId = expectedGroups[2].Id
                });
                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = expectedGroups[2],
                    PrincipalId = expectedGroups[3].Id
                });
                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = expectedGroups[3],
                    PrincipalId = expectedGroups[4].Id
                });
                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = expectedGroups[4],
                    PrincipalId = expectedGroups[5].Id
                });
                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = expectedGroups[4],
                    PrincipalId = expectedGroups[6].Id
                });
                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = expectedGroups[6],
                    PrincipalId = expectedGroups[7].Id
                });
                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = expectedGroups[6],
                    PrincipalId = expectedGroups[8].Id
                });
                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = expectedGroups[8],
                    PrincipalId = expectedGroups[9].Id
                });

                await authorizationsFixture.Context.SaveChangesAsync();

                var targettedGroup = await authorizationsFixture.Context.Groups().FirstOrDefaultAsync(g => g.Name == expectedGroups[9].Name);
                var parentLink = await authorizationsFixture.AuthorizationsClient.GetGroupParentLinkAsync(targettedGroup.Id);

                string[] expectedParentLinkItems = { "group8", "group6", "group4", "group3", "group2", "group0" };
                Assert.Equal(expectedParentLinkItems.Length, parentLink.Count);
                foreach (var parentLinkItem in parentLink)
                {
                    var group = await authorizationsFixture.Context.Groups().FirstOrDefaultAsync(g => g.Id == parentLinkItem);
                    Assert.True(expectedParentLinkItems.Contains(group.Name));
                }
            }
        }

        [Fact]
        public async Task GetGroupMembers_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                var groupParent = new Group { Name = "groupParent" };
                var groupChild1 = new Group { Id = Guid.NewGuid(), Name = "groupChild1" };
                var groupChild2 = new Group { Id = Guid.NewGuid(), Name = "groupChild2" };

                authorizationsFixture.Context.Groups().Add(groupParent);
                authorizationsFixture.Context.Groups().Add(groupChild1);
                authorizationsFixture.Context.Groups().Add(groupChild2);

                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = groupParent,
                    PrincipalId = groupChild1.Id
                });

                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = groupParent,
                    PrincipalId = groupChild2.Id
                });

                await authorizationsFixture.Context.SaveChangesAsync();

                var child1 = await authorizationsFixture.Context.Groups().FirstAsync(g => g.Name == "groupChild1");
                var child2 = await authorizationsFixture.Context.Groups().FirstAsync(g => g.Name == "groupChild2");
                var childrenIds = await authorizationsFixture.AuthorizationsManager.GetGroupMembersAsync("groupParent");
                Assert.True(childrenIds.Contains(child1.Id) && childrenIds.Contains(child2.Id));
            }
        }

        [Fact]
        public async Task HasMembership_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                var groupParent = new Group { Name = "groupParent" };

                authorizationsFixture.Context.Groups().Add(groupParent);

                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = groupParent,
                    PrincipalId = authorizationsFixture.Context.CurrentUserId
                });

                await authorizationsFixture.Context.SaveChangesAsync();

                Assert.True(await authorizationsFixture.AuthorizationsClient.HasMembershipAsync("groupParent"));
            }
        }

        [Fact]
        public async Task HasMembershipForUsers_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                var groupParent = new Group { Name = "groupParent" };

                authorizationsFixture.Context.Groups().Add(groupParent);

                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = groupParent,
                    PrincipalId = authorizationsFixture.Context.CurrentUserId
                });

                await authorizationsFixture.Context.SaveChangesAsync();

                var usersInGroup = await authorizationsFixture.AuthorizationsManager.HasMembershipAsync(new List<Guid> { authorizationsFixture.Context.CurrentUserId }, "groupParent");
                Assert.True(usersInGroup.Contains(authorizationsFixture.Context.CurrentUserId));
            }
        }

        [Fact]
        public async Task DetectMemberships_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                var group1 = new Group { Name = "group1" };
                var group2 = new Group { Name = "group2" };
                var group3 = new Group { Name = "group3" };

                authorizationsFixture.Context.Groups().Add(group1);
                authorizationsFixture.Context.Groups().Add(group2);
                authorizationsFixture.Context.Groups().Add(group3);

                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = group2,
                    PrincipalId = authorizationsFixture.Context.CurrentUserId
                });

                await authorizationsFixture.Context.SaveChangesAsync();

                var userMemberships = await authorizationsFixture.AuthorizationsClient.DetectMembershipsAsync(new List<string> { "group1", "group2" });
                Assert.True(userMemberships.Contains("group2"));

            }
        }

        [Fact]
        public async Task DeleteGroup_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                var parent = new Group { Name = "group1" };
                var child = new Group { Id = Guid.NewGuid(), Name = "group2" };
                authorizationsFixture.Context.Groups().Add(parent);
                authorizationsFixture.Context.Groups().Add(child);
                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = parent,
                    PrincipalId = child.Id
                });
                await authorizationsFixture.Context.SaveChangesAsync();

                var group1FromDb = await authorizationsFixture.Context.Groups().FirstAsync(g => g.Name == "group1");
                var group2FromDb = await authorizationsFixture.Context.Groups().FirstAsync(g => g.Name == "group2");
                await authorizationsFixture.AuthorizationsManager.DeleteGroupAsync("group1", withChildren: false);
                await authorizationsFixture.Context.SaveChangesAsync();

                Assert.Null(authorizationsFixture.Context.Groups().FirstOrDefault(r => r.Name == "group1"));
                Assert.Null(await authorizationsFixture.Context.Principals().FirstOrDefaultAsync(p => p.Id == group1FromDb.Id));
                Assert.NotNull(authorizationsFixture.Context.Groups().FirstOrDefault(r => r.Name == "group2"));
                Assert.NotNull(await authorizationsFixture.Context.Principals().FirstOrDefaultAsync(p => p.Id == group2FromDb.Id));
            }
        }

        [Fact]
        public async Task DeleteGroupWithChildren_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                var parent = new Group { Name = "group1" };
                var child = new Group { Id = Guid.NewGuid(), Name = "group2" };
                authorizationsFixture.Context.Groups().Add(parent);
                authorizationsFixture.Context.Groups().Add(child);
                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = parent,
                    PrincipalId = child.Id
                });
                await authorizationsFixture.Context.SaveChangesAsync();

                var group1FromDb = await authorizationsFixture.Context.Groups().FirstAsync(g => g.Name == "group1");
                var group2FromDb = await authorizationsFixture.Context.Groups().FirstAsync(g => g.Name == "group2");
                await authorizationsFixture.AuthorizationsManager.DeleteGroupAsync("group1");
                await authorizationsFixture.Context.SaveChangesAsync();

                Assert.Null(authorizationsFixture.Context.Groups().FirstOrDefault(r => r.Name == "group1"));
                Assert.Null(await authorizationsFixture.Context.Principals().FirstOrDefaultAsync(p => p.Id == group1FromDb.Id));
                Assert.Null(authorizationsFixture.Context.Groups().FirstOrDefault(r => r.Name == "group2"));
                Assert.Null(await authorizationsFixture.Context.Principals().FirstOrDefaultAsync(p => p.Id == group1FromDb.Id));
            }
        }

        [Fact]
        public async Task AddGroupToGroup_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await authorizationsFixture.AuthorizationsManager.AddGroupToGroupAsync("childGroup", "parentGroup");

                await authorizationsFixture.Context.SaveChangesAsync();

                var childGroup = await authorizationsFixture.Context.Groups().FirstOrDefaultAsync(g => g.Name == "childGroup");
                var parentGroup = await authorizationsFixture.Context.Groups().FirstOrDefaultAsync(g => g.Name == "parentGroup");
                Assert.NotNull(childGroup);
                Assert.NotNull(parentGroup);
                Assert.NotNull(await authorizationsFixture.Context.Memberships().FirstOrDefaultAsync(m => m.PrincipalId == childGroup.Id && m.GroupId == parentGroup.Id));
            }
        }
    }
}
