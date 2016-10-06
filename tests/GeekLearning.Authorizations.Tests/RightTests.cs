using GeekLearning.Authorizations.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GeekLearning.Authorizations.Tests
{
    public class RightTests : IClassFixture<AuthorizationsFixture>
    {
        private AuthorizationsFixture authorizationsFixture;

        public RightTests(AuthorizationsFixture authorizationsFixture)
        {
            this.authorizationsFixture = authorizationsFixture;
        }

        [Fact]
        public async Task CreateRight_ShouldBeOk()
        {
            await this.authorizationsFixture.AuthorizationsProvisioningClient.CreateRightAsync("right1");

            Assert.NotNull(this.authorizationsFixture.Context.Set<Right>().FirstOrDefault(r => r.Name == "right1"));
        }
    }
}
