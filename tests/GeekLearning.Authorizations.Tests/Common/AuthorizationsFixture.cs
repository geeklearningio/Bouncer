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

        public AuthorizationsFixture(string databaseName, bool mockProvisioning = false)
        {
            var builder = new DbContextOptionsBuilder<AuthorizationsTestContext>();

            databaseName = $"{ databaseName ?? "TestDatabase"}.db";

            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = databaseName };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);

            builder.UseSqlite(connection);

            this.Context = new AuthorizationsTestContext(builder.Options);

            this.Context.Database.OpenConnection();
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

        public AuthorizationsFixture(RightsResult rightsResult, string databaseName, bool mockProvisioning = false) : this(databaseName, mockProvisioning)
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
