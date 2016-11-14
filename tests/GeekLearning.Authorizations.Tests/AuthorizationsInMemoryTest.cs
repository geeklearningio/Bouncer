namespace GeekLearning.Authorizations.Tests
{
    using GeekLearning.Authorizations.Data;
    using GeekLearning.Authorizations.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Xunit;
    using GeekLearning.Authorizations.Model;

    public class AuthorizationsInMemoryTest
    {
        [Fact]
        public async Task AffectRoleOnScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture(new RightsResult(), mockProvisioning: true))
            {
                await authorizationsFixture.AuthorizationsProvisioningClient.CreateRoleAsync("role1", new string[] { "right1", "right2" });

                await authorizationsFixture.AuthorizationsProvisioningClient.CreateScopeAsync("scope1", "Scope 1");

                await authorizationsFixture.AuthorizationsProvisioningClient
                                           .AffectRoleToPrincipalOnScopeAsync(
                                               "role1",
                                               authorizationsFixture.Context.CurrentUserId,
                                               "scope1");


                await authorizationsFixture.AuthorizationsProvisioningClient.CreateScopeAsync("scope2", "Scope 2", new string[] { "scope1" });

                var results = await authorizationsFixture.AuthorizationsClient.GetRightsAsync("scope2");

                Assert.True(results.HasRightOnScope("right1", "scope2"));
                Assert.Equal("scope1", results.RightsPerScope["scope1"].ScopeHierarchies.First());
                Assert.Equal("scope1/scope2", results.RightsPerScope["scope2"].ScopeHierarchies.First());

            }
        }       
    }
}
