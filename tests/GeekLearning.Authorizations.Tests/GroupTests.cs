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
                await authorizationsFixture.AuthorizationsProvisioningClient.CreateGroupAsync("group1");

                await authorizationsFixture.Context.SaveChangesAsync();

                var group = authorizationsFixture.Context.Groups()
                                                        .FirstOrDefault(r => r.Name == "group1");
                Assert.NotNull(group);
            }
        }

        [Fact]
        public async Task DeleteGroup_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                authorizationsFixture.Context.Groups().Add(new Group { Name = "group1" });

                await authorizationsFixture.Context.SaveChangesAsync();

                await authorizationsFixture.AuthorizationsProvisioningClient.DeleteGroupAsync("group1");

                await authorizationsFixture.Context.SaveChangesAsync();

                Assert.Null(authorizationsFixture.Context.Groups().FirstOrDefault(r => r.Name == "group1"));
            }
        }
    }
}
