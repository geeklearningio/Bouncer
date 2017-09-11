namespace GeekLearning.Authorizations.Tests
{
    using EntityFrameworkCore;
    using EntityFrameworkCore.Data;
    using Microsoft.EntityFrameworkCore;
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
                await authorizationsFixture.AuthorizationsProvisioningClient.CreateGroupAsync("group2", parentGroupName: "group1");
             
                await authorizationsFixture.Context.SaveChangesAsync();

                var group = authorizationsFixture.Context
                    .Groups()
                    .FirstOrDefault(r => r.Name == "group2");
                Assert.NotNull(group);

                var membership = authorizationsFixture.Context.Memberships().FirstOrDefault(m => m.Group.Name == "group1");
                Assert.Equal("group2", ((Group)membership.Principal).Name);
            }
        }

        [Fact]
        public async Task DeleteGroup_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                var parent = new Group { Name = "group1" };
                var child = new Group { Name = "group2" };
                authorizationsFixture.Context.Groups().Add(parent);
                authorizationsFixture.Context.Groups().Add(child);
                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = parent,
                    Principal = child
                });
                await authorizationsFixture.Context.SaveChangesAsync();

                var group1FromDb = await authorizationsFixture.Context.Groups().FirstAsync(g => g.Name == "group1");
                var group2FromDb = await authorizationsFixture.Context.Groups().FirstAsync(g => g.Name == "group2");
                await authorizationsFixture.AuthorizationsProvisioningClient.DeleteGroupAsync("group1", withChildren: false);
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
                var child = new Group { Name = "group2" };
                authorizationsFixture.Context.Groups().Add(parent);
                authorizationsFixture.Context.Groups().Add(child);
                authorizationsFixture.Context.Memberships().Add(new Membership
                {
                    Group = parent,
                    Principal = child
                });
                await authorizationsFixture.Context.SaveChangesAsync();

                var group1FromDb = await authorizationsFixture.Context.Groups().FirstAsync(g => g.Name == "group1");
                var group2FromDb = await authorizationsFixture.Context.Groups().FirstAsync(g => g.Name == "group2");
                await authorizationsFixture.AuthorizationsProvisioningClient.DeleteGroupAsync("group1");
                await authorizationsFixture.Context.SaveChangesAsync();

                Assert.Null(authorizationsFixture.Context.Groups().FirstOrDefault(r => r.Name == "group1"));
                Assert.Null(await authorizationsFixture.Context.Principals().FirstOrDefaultAsync(p => p.Id == group1FromDb.Id));
                Assert.Null(authorizationsFixture.Context.Groups().FirstOrDefault(r => r.Name == "group2"));
                Assert.Null(await authorizationsFixture.Context.Principals().FirstOrDefaultAsync(p => p.Id == group1FromDb.Id));
            }
        }
    }
}
