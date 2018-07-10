namespace GeekLearning.Bouncer.Tests
{
    using EntityFrameworkCore;
    using EntityFrameworkCore.Data;
    using GeekLearning.Bouncer.EntityFrameworkCore.Data.Extensions;
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
            using (var bouncerFixture = new BouncerFixture())
            {
                await bouncerFixture.AuthorizationsManager.CreateGroupAsync("group2", parentGroupName: "group1");

                await bouncerFixture.Context.SaveChangesAsync();

                var group = bouncerFixture.Context
                    .Groups
                    .FirstOrDefault(r => r.Name == "group2");
                Assert.NotNull(group);

                var membership = bouncerFixture.Context.Memberships.FirstOrDefault(m => m.Group.Name == "group1");
                group = bouncerFixture.Context.Groups.FirstOrDefault(g => g.Id == membership.PrincipalId);
                Assert.Equal("group2", group.Name);
            }
        }

        [Fact]
        public async Task AddPrincipalToGroup_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                var parentPrincipal = new Principal
                {
                    Id = Guid.NewGuid(),
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };
                var parentGroup = new Group(parentPrincipal) { Name = "group1" };
                bouncerFixture.Context.Groups.Add(parentGroup);

                var childPrincipal = new Principal
                {
                    Id = Guid.NewGuid(),
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };
                var childGroup = new Group(childPrincipal) { Name = "group2" };
                bouncerFixture.Context.Groups.Add(childGroup);

                bouncerFixture.Context.Memberships.Add(new Membership
                {
                    Group = parentGroup,
                    PrincipalId = childGroup.Id,
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                });

                var scope = new Scope
                {
                    Name = "Scope1",
                    Description = "Scope1",
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };
                bouncerFixture.Context.Scopes.Add(scope);

                var right = new Right
                {
                    Name = "Right1",
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };
                bouncerFixture.Context.Rights.Add(right);

                var role = new Role
                {
                    Name = "Role1",
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };
                bouncerFixture.Context.Roles.Add(role);

                bouncerFixture.Context.RoleRights.Add(new RoleRight { Role = role, Right = right });

                bouncerFixture.Context.Authorizations.Add(new Authorization
                {
                    Scope = scope,
                    Role = role,
                    PrincipalId = childGroup.Id,
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                });

                await bouncerFixture.Context.SaveChangesAsync();

                var group = bouncerFixture.Context
                    .Groups
                    .FirstOrDefault(r => r.Name == "group2");

                await bouncerFixture.AuthorizationsManager
                    .AddPrincipalToGroupAsync(bouncerFixture.Context.CurrentUserId, "group2");

                await bouncerFixture.Context.SaveChangesAsync();

                var membership = bouncerFixture.Context.Memberships.FirstOrDefault(m => m.PrincipalId == bouncerFixture.Context.CurrentUserId);
                Assert.NotNull(membership);

                await bouncerFixture.AuthorizationsManager
                    .RemovePrincipalFromGroupAsync(bouncerFixture.Context.CurrentUserId, "group2");
                await bouncerFixture.AuthorizationsManager
                    .AddPrincipalToGroupAsync(bouncerFixture.Context.CurrentUserId, "group2");

                await bouncerFixture.Context.SaveChangesAsync();

                membership = bouncerFixture.Context.Memberships.FirstOrDefault(m => m.PrincipalId == bouncerFixture.Context.CurrentUserId);
                Assert.NotNull(membership);
            }
        }

        [Fact]
        public async Task GetGroupParentLink_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                List<Group> expectedGroups = new List<Group>();
                for (int i = 0; i < 10; i++)
                {
                    var principal = new Principal
                    {
                        Id = Guid.NewGuid(),
                        CreationBy = bouncerFixture.Context.CurrentUserId,
                        ModificationBy = bouncerFixture.Context.CurrentUserId
                    };
                    var group = new Group(principal) { Name = "group" + i };
                    expectedGroups.Add(group);
                    bouncerFixture.Context.Groups.Add(group);
                }

                bouncerFixture.Context.Memberships.Add(new Membership
                {
                    Group = expectedGroups[0],
                    PrincipalId = expectedGroups[1].Id,
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                });
                bouncerFixture.Context.Memberships.Add(new Membership
                {
                    Group = expectedGroups[0],
                    PrincipalId = expectedGroups[2].Id,
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                });
                bouncerFixture.Context.Memberships.Add(new Membership
                {
                    Group = expectedGroups[2],
                    PrincipalId = expectedGroups[3].Id,
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                });
                bouncerFixture.Context.Memberships.Add(new Membership
                {
                    Group = expectedGroups[3],
                    PrincipalId = expectedGroups[4].Id,
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                });
                bouncerFixture.Context.Memberships.Add(new Membership
                {
                    Group = expectedGroups[4],
                    PrincipalId = expectedGroups[5].Id,
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                });
                bouncerFixture.Context.Memberships.Add(new Membership
                {
                    Group = expectedGroups[4],
                    PrincipalId = expectedGroups[6].Id,
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                });
                bouncerFixture.Context.Memberships.Add(new Membership
                {
                    Group = expectedGroups[6],
                    PrincipalId = expectedGroups[7].Id,
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                });
                bouncerFixture.Context.Memberships.Add(new Membership
                {
                    Group = expectedGroups[6],
                    PrincipalId = expectedGroups[8].Id,
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                });
                bouncerFixture.Context.Memberships.Add(new Membership
                {
                    Group = expectedGroups[8],
                    PrincipalId = expectedGroups[9].Id,
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                });

                await bouncerFixture.Context.SaveChangesAsync();

                var targettedGroup = await bouncerFixture.Context.Groups.FirstOrDefaultAsync(g => g.Name == expectedGroups[9].Name);
                var parentLink = await bouncerFixture.AuthorizationsClient.GetGroupParentLinkAsync(targettedGroup.Id);

                string[] expectedParentLinkItems = { "group8", "group6", "group4", "group3", "group2", "group0" };
                Assert.Equal(expectedParentLinkItems.Length, parentLink.Count);
                foreach (var parentLinkItem in parentLink)
                {
                    var group = await bouncerFixture.Context.Groups.FirstOrDefaultAsync(g => g.Id == parentLinkItem);
                    Assert.Contains(group.Name, expectedParentLinkItems);
                }
            }
        }

        [Fact]
        public async Task GetGroupMembers_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                var parentPrincipal = new Principal
                {
                    Id = Guid.NewGuid(),
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };
                var groupParent = new Group(parentPrincipal) { Name = "groupParent" };
                bouncerFixture.Context.Groups.Add(groupParent);

                var groupChild1Principal = new Principal
                {
                    Id = Guid.NewGuid(),
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };
                var groupChild1 = new Group(groupChild1Principal) { Name = "groupChild1" };
                bouncerFixture.Context.Groups.Add(groupChild1);

                var groupChild2Principal = new Principal
                {
                    Id = Guid.NewGuid(),
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };
                var groupChild2 = new Group(groupChild2Principal) { Name = "groupChild2" };
                bouncerFixture.Context.Groups.Add(groupChild2);
                
                bouncerFixture.Context.Memberships.Add(new Membership
                {
                    Group = groupParent,
                    PrincipalId = groupChild1.Id,
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                });

                bouncerFixture.Context.Memberships.Add(new Membership
                {
                    Group = groupParent,
                    PrincipalId = groupChild2.Id,
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                });

                await bouncerFixture.Context.SaveChangesAsync();

                var child1 = await bouncerFixture.Context.Groups.FirstAsync(g => g.Name == "groupChild1");
                var child2 = await bouncerFixture.Context.Groups.FirstAsync(g => g.Name == "groupChild2");
                var childrenIds = await bouncerFixture.AuthorizationsManager.GetGroupMembersAsync("groupParent");
                Assert.True(childrenIds.Contains(child1.Id) && childrenIds.Contains(child2.Id));
            }
        }

        [Fact]
        public async Task HasMembership_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                var principal = new Principal
                {
                    Id = Guid.NewGuid(),
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };
                var groupParent = new Group(principal) { Name = "groupParent" };
                bouncerFixture.Context.Groups.Add(groupParent);

                bouncerFixture.Context.Memberships.Add(new Membership
                {
                    Group = groupParent,
                    PrincipalId = bouncerFixture.Context.CurrentUserId,
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                });

                await bouncerFixture.Context.SaveChangesAsync();

                Assert.True(await bouncerFixture.AuthorizationsClient.HasMembershipAsync("groupParent"));
            }
        }

        [Fact]
        public async Task HasMembershipForUsers_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                var principal = new Principal
                {
                    Id = Guid.NewGuid(),
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };
                var groupParent = new Group(principal) { Name = "groupParent" };
                bouncerFixture.Context.Groups.Add(groupParent);

                bouncerFixture.Context.Memberships.Add(new Membership
                {
                    Group = groupParent,
                    PrincipalId = bouncerFixture.Context.CurrentUserId,
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                });

                await bouncerFixture.Context.SaveChangesAsync();

                var usersInGroup = await bouncerFixture.AuthorizationsManager.HasMembershipAsync(new List<Guid> { bouncerFixture.Context.CurrentUserId }, "groupParent");
                Assert.True(usersInGroup.Contains(bouncerFixture.Context.CurrentUserId));
            }
        }

        [Fact]
        public async Task DetectMemberships_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                var group1Principal = new Principal
                {
                    Id = Guid.NewGuid(),
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };
                var group1 = new Group(group1Principal) { Name = "group1" };
                bouncerFixture.Context.Groups.Add(group1);

                var group2Principal = new Principal
                {
                    Id = Guid.NewGuid(),
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };
                var group2 = new Group(group2Principal) { Name = "group2" };
                bouncerFixture.Context.Groups.Add(group2);

                var group3Principal = new Principal
                {
                    Id = Guid.NewGuid(),
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };
                var group3 = new Group(group3Principal) { Name = "group3" };
                bouncerFixture.Context.Groups.Add(group3);

                bouncerFixture.Context.Memberships.Add(new Membership
                {
                    Group = group2,
                    PrincipalId = bouncerFixture.Context.CurrentUserId,
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                });

                await bouncerFixture.Context.SaveChangesAsync();

                var userMemberships = await bouncerFixture.AuthorizationsClient.DetectMembershipsAsync(
                    new List<string> { "group1", "group2" });
                Assert.True(userMemberships.Contains("group2"));
            }
        }

        [Fact]
        public async Task DeleteGroup_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                var parentPrincipal = new Principal
                {
                    Id = Guid.NewGuid(),
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };
                var parent = new Group(parentPrincipal) { Name = "group1" };
                bouncerFixture.Context.Groups.Add(parent);

                var childPrincipal = new Principal
                {
                    Id = Guid.NewGuid(),
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };
                var child = new Group(childPrincipal) { Name = "group2" };                
                bouncerFixture.Context.Groups.Add(child);

                bouncerFixture.Context.Memberships.Add(new Membership
                {
                    Group = parent,
                    PrincipalId = child.Id,
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                });
                await bouncerFixture.Context.SaveChangesAsync();

                var group1FromDb = await bouncerFixture.Context.Groups.FirstAsync(g => g.Name == "group1");
                var group2FromDb = await bouncerFixture.Context.Groups.FirstAsync(g => g.Name == "group2");
                await bouncerFixture.AuthorizationsManager.DeleteGroupAsync("group1", withChildren: false);
                await bouncerFixture.Context.SaveChangesAsync();

                Assert.Null(bouncerFixture.Context.Groups.FirstOrDefault(r => r.Name == "group1"));
                Assert.Null(await bouncerFixture.Context.Principals.FirstOrDefaultAsync(p => p.Id == group1FromDb.Id));
                Assert.NotNull(bouncerFixture.Context.Groups.FirstOrDefault(r => r.Name == "group2"));
                Assert.NotNull(await bouncerFixture.Context.Principals.FirstOrDefaultAsync(p => p.Id == group2FromDb.Id));
            }
        }

        [Fact]
        public async Task DeleteGroupWithChildren_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                var parentPrincipal = new Principal
                {
                    Id = Guid.NewGuid(),
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };
                var parent = new Group(parentPrincipal) { Name = "group1" };
                bouncerFixture.Context.Groups.Add(parent);

                var childPrincipal = new Principal
                {
                    Id = Guid.NewGuid(),
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                };
                var child = new Group(childPrincipal) { Name = "group2" };
                bouncerFixture.Context.Groups.Add(child);

                bouncerFixture.Context.Memberships.Add(new Membership
                {
                    Group = parent,
                    PrincipalId = child.Id,
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId
                });
                await bouncerFixture.Context.SaveChangesAsync();

                var group1FromDb = await bouncerFixture.Context.Groups.FirstAsync(g => g.Name == "group1");
                var group2FromDb = await bouncerFixture.Context.Groups.FirstAsync(g => g.Name == "group2");
                await bouncerFixture.AuthorizationsManager.DeleteGroupAsync("group1");
                await bouncerFixture.Context.SaveChangesAsync();

                Assert.Null(bouncerFixture.Context.Groups.FirstOrDefault(r => r.Name == "group1"));
                Assert.Null(await bouncerFixture.Context.Principals.FirstOrDefaultAsync(p => p.Id == group1FromDb.Id));
                Assert.Null(bouncerFixture.Context.Groups.FirstOrDefault(r => r.Name == "group2"));
                Assert.Null(await bouncerFixture.Context.Principals.FirstOrDefaultAsync(p => p.Id == group1FromDb.Id));
            }
        }

        [Fact]
        public async Task AddGroupToGroup_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                await bouncerFixture.AuthorizationsManager.AddGroupToGroupAsync("childGroup", "parentGroup");

                await bouncerFixture.Context.SaveChangesAsync();

                var childGroup = await bouncerFixture.Context.Groups.FirstOrDefaultAsync(g => g.Name == "childGroup");
                var parentGroup = await bouncerFixture.Context.Groups.FirstOrDefaultAsync(g => g.Name == "parentGroup");
                Assert.NotNull(childGroup);
                Assert.NotNull(parentGroup);
                Assert.NotNull(await bouncerFixture.Context.Memberships.FirstOrDefaultAsync(m => m.PrincipalId == childGroup.Id && m.GroupId == parentGroup.Id));
            }
        }
    }
}
