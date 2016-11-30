namespace GeekLearning.Authorizations.Tests
{
    using GeekLearning.Authorizations.EntityFrameworkCore;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Model;
    using System;
    using Testing;

    public sealed class AuthorizationsFixture : IDisposable
    {
        private UserRightsProviderService userRightsProviderService = new UserRightsProviderService();

        public AuthorizationsTestContext Context { get; private set; }

        public IAuthorizationsProvisioningClient AuthorizationsProvisioningClient { get; private set; }

        public IAuthorizationsClient AuthorizationsClient { get; private set; }

        public AuthorizationsFixture(bool mockProvisioning = false)
        {
            var builder = new DbContextOptionsBuilder<AuthorizationsTestContext>();

            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = "TestDatabase.db" };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);

            builder.UseSqlite(connection);

            Context = new AuthorizationsTestContext(builder.Options);

            Context.Database.EnsureDeleted();
            Context.Database.OpenConnection();
            Context.Database.EnsureCreated();

            Context.Seed();

            if (mockProvisioning)
            {
                this.AuthorizationsProvisioningClient = new AuthorizationsProvisioningTestClient(this.userRightsProviderService);
            }
            else
            {
                this.AuthorizationsProvisioningClient = new AuthorizationsProvisioningClient<AuthorizationsTestContext>(Context, new PrincipalIdProvider(Context));
            }

            this.AuthorizationsClient = new AuthorizationsClient<AuthorizationsTestContext>(Context, new PrincipalIdProvider(Context));
        }

        public AuthorizationsFixture(RightsResult rightsResult, bool mockProvisioning = false) : this(mockProvisioning)
        {
            this.AuthorizationsClient = new AuthorizationsTestClient(this.userRightsProviderService, rightsResult);
        }

        public void Dispose()
        {
            this.Context.Dispose();
        }
    }
}
