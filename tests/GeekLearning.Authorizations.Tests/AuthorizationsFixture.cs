namespace GeekLearning.Authorizations.Tests
{
    using System;
    using GeekLearning.Authorizations.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public sealed class AuthorizationsFixture : IDisposable
    {
        public AuthorizationsTestContext Context =>
            new AuthorizationsTestContext();

        public IAuthorizationsProvisioningClient AuthorizationsProvisioningClient =>
            new AuthorizationsProvisioningClient<AuthorizationsTestContext>(Context, Context.CurrentUserId);

        public IAuthorizationsClient AuthorizationsClient =>
            new AuthorizationsClient<AuthorizationsTestContext>(Context, Context.CurrentUserId);

        public AuthorizationsFixture()
        {
            Context.Database.OpenConnection();
            Context.Database.EnsureCreated();

            Context.Seed();
        }

        public void Dispose()
        {
            this.Context.Dispose();
        }
    }
}
