using GeekLearning.Authorizations.Data;
using GeekLearning.Authorizations.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GeekLearning.Authorizations.Tests
{
    public class AuthorizationsTest
    {
        [Fact]
        public async Task AffectRoleOnScope_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                authorizationsFixture.Context.Roles().Add(new Role { Name = "role1" });

                authorizationsFixture.Context.Scopes().Add(new Scope { Name = "scope1", Description = "Scope 1"});

                authorizationsFixture.Context.SaveChanges();

                await authorizationsFixture.AuthorizationsProvisioningClient
                                           .AffectRoleToPrincipalOnScopeAsync(
                                                "role1",
                                                authorizationsFixture.Context.CurrentUserId,
                                                "scope1");

                var authorization = authorizationsFixture.Context.Authorizations()
                                                                 .Include(a => a.Scope)
                                                                 .Include(a => a.Role)
                                                                 .FirstOrDefault(a => a.PrincipalId == authorizationsFixture.Context.CurrentUserId);

                Assert.NotNull(authorization);
                Assert.Equal("role1", authorization.Role.Name);
                Assert.Equal("scope1", authorization.Scope.Name);
            }
        }
    }
}
