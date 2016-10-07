namespace GeekLearning.Authorizations.Tests
{
    using GeekLearning.Authorizations.Data;
    using GeekLearning.Authorizations.EntityFrameworkCore;
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
                await authorizationsFixture.AuthorizationsProvisioningClient.CreateRightAsync("right1");

                Assert.NotNull(authorizationsFixture.Context.Rights().FirstOrDefault(r => r.Name == "right1"));
            }
        }


        [Fact]
        public async Task DeleteRight_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                authorizationsFixture.Context.Rights().Add(new Right { Name = "right1" });

                authorizationsFixture.Context.SaveChanges();

                await authorizationsFixture.AuthorizationsProvisioningClient.DeleteRightAsync("right1");

                Assert.Null(authorizationsFixture.Context.Rights().FirstOrDefault(r => r.Name == "right1"));
            }
        }
    }
}
