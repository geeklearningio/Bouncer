using GeekLearning.Authorizations.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GeekLearning.Authorizations.Tests
{
    public class RightTests
    {
        [Fact]
        public async Task CreateRight_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                await authorizationsFixture.AuthorizationsProvisioningClient.CreateRightAsync("right1");

                Assert.NotNull(authorizationsFixture.Context.Set<Right>().FirstOrDefault(r => r.Name == "right1"));
            }
        }


        [Fact]
        public async Task DeleteRight_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                authorizationsFixture.Context.Set<Right>().Add(new Right { Name = "right1" });

                authorizationsFixture.Context.SaveChanges();

                await authorizationsFixture.AuthorizationsProvisioningClient.DeleteRightAsync("right1");

                Assert.Null(authorizationsFixture.Context.Set<Right>().FirstOrDefault(r => r.Name == "right1"));
            }
        }
    }
}
