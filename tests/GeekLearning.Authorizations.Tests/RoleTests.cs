namespace GeekLearning.Authorizations.Tests
{
    using GeekLearning.Authorizations.Data;
    using GeekLearning.Authorizations.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;
    using Microsoft.EntityFrameworkCore;

    public class RoleTests
    {
        [Fact]
        public async Task CreateRole_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await authorizationsFixture.AuthorizationsProvisioningClient
                                            .CreateRoleAsync(
                                                "role1",
                                                new string[] { "right1", "right2" });

                var role = authorizationsFixture.Context.Roles()
                                                        .Include(r => r.Rights)
                                                        .ThenInclude(rr => rr.Right)
                                                        .FirstOrDefault(r => r.Name == "role1");
                Assert.NotNull(role);

                var rightKeys = role.Rights.Select(r => r.Right.Name);
                Assert.True(rightKeys.Contains("right1"));
                Assert.True(rightKeys.Contains("right2"));
            }
        }

        [Fact]
        public async Task DeleteRole_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                authorizationsFixture.Context.Roles().Add(new Role { Name = "role1" });

                authorizationsFixture.Context.SaveChanges();

                await authorizationsFixture.AuthorizationsProvisioningClient.DeleteRoleAsync("role1");

                Assert.Null(authorizationsFixture.Context.Roles().FirstOrDefault(r => r.Name == "role1"));
            }
        }
    }
}
