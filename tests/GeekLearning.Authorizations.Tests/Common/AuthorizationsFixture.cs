namespace GeekLearning.Authorizations.Tests
{
    using EntityFrameworkCore;
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

            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);

            connection.Open();

            builder.UseSqlite(connection);

            this.Context = new AuthorizationsTestContext(builder.Options);

            this.Context.Database.EnsureCreated();

            this.Context.Seed();

            if (mockProvisioning)
            {
                this.AuthorizationsProvisioningClient = new AuthorizationsProvisioningTestClient(this.userRightsProviderService);
            }
            else
            {
                this.AuthorizationsProvisioningClient = new AuthorizationsProvisioningClient<AuthorizationsTestContext>(this.Context, new PrincipalIdProvider(this.Context));
            }

            this.AuthorizationsClient = new AuthorizationsClient<AuthorizationsTestContext>(this.Context, new PrincipalIdProvider(this.Context));
        }

        public AuthorizationsFixture(RightsResult rightsResult, bool mockProvisioning = false) : this(mockProvisioning)
        {
            this.AuthorizationsClient = new AuthorizationsTestClient(this.userRightsProviderService, rightsResult);
        }

        public void Dispose()
        {
            this.Context.Database.CloseConnection();
            this.Context.Database.EnsureDeleted();
            this.Context.Dispose();
        }
    }
}
