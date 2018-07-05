namespace Bouncer.Tests
{
    using EntityFrameworkCore;
    using EntityFrameworkCore.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class RightTests
    {
        [Fact]
        public async Task CreateRight_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await authorizationsFixture.AuthorizationsManager.CreateRightAsync("right1");

                await authorizationsFixture.Context.SaveChangesAsync();

                Assert.NotNull(authorizationsFixture.Context.Rights().FirstOrDefault(r => r.Name == "right1"));
            }
        }

        [Fact]
        public async Task DeleteRight_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                authorizationsFixture.Context.Rights().Add(
                    new Right
                    {
                        Name = "right1",
                        CreationBy = authorizationsFixture.Context.CurrentUserId,
                        ModificationBy = authorizationsFixture.Context.CurrentUserId
                    });

                await authorizationsFixture.Context.SaveChangesAsync();

                await authorizationsFixture.AuthorizationsManager.DeleteRightAsync("right1");

                await authorizationsFixture.Context.SaveChangesAsync();

                Assert.Null(authorizationsFixture.Context.Rights().FirstOrDefault(r => r.Name == "right1"));
            }
        }
    }
}
