namespace GeekLearning.Authorizations.Tests
{
    using EntityFrameworkCore;
    using EntityFrameworkCore.Data;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class RoleTests
    {
        [Fact]
        public async Task CreateRole_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await authorizationsFixture.AuthorizationsManager
                                           .CreateRoleAsync(
                                               "role1",
                                               new string[] { "right1", "right2" });

                await authorizationsFixture.Context.SaveChangesAsync();

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
                authorizationsFixture.Context.Roles().Add(
                    new Role
                    {
                        Name = "role1",
                        CreationBy = authorizationsFixture.Context.CurrentUserId,
                        ModificationBy = authorizationsFixture.Context.CurrentUserId
                    });

                await authorizationsFixture.Context.SaveChangesAsync();

                await authorizationsFixture.AuthorizationsManager.DeleteRoleAsync("role1");

                await authorizationsFixture.Context.SaveChangesAsync();

                Assert.Null(authorizationsFixture.Context.Roles().FirstOrDefault(r => r.Name == "role1"));
            }
        }
    }
}
