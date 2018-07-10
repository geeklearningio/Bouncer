namespace GeekLearning.Bouncer.Tests
{
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
            using (var bouncerFixture = new BouncerFixture())
            {
                await bouncerFixture.AuthorizationsManager
                        .CreateRoleAsync("role1", new string[] { "right1", "right2" });

                await bouncerFixture.Context.SaveChangesAsync();

                var role = bouncerFixture.Context.Roles
                            .Include(r => r.Rights)
                            .ThenInclude(rr => rr.Right)
                            .FirstOrDefault(r => r.Name == "role1");
                Assert.NotNull(role);

                var rightKeys = role.Rights.Select(r => r.Right.Name);
                Assert.Contains("right1", rightKeys);
                Assert.Contains("right2", rightKeys);
            }
        }

        [Fact]
        public async Task DeleteRole_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                bouncerFixture.Context.Roles.Add(
                    new Role
                    {
                        Name = "role1",
                        CreationBy = bouncerFixture.Context.CurrentUserId,
                        ModificationBy = bouncerFixture.Context.CurrentUserId
                    });

                await bouncerFixture.Context.SaveChangesAsync();

                await bouncerFixture.AuthorizationsManager.DeleteRoleAsync("role1");

                await bouncerFixture.Context.SaveChangesAsync();

                Assert.Null(bouncerFixture.Context.Roles.FirstOrDefault(r => r.Name == "role1"));
            }
        }
    }
}
