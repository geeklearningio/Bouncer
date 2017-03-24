namespace GeekLearning.Authorizations.Tests
{
    using EntityFrameworkCore;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using System;

    public sealed class AuthorizationsFixture : IDisposable
    {
        public AuthorizationsFixture()
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

            this.AuthorizationsProvisioningClient = new AuthorizationsProvisioningClient<AuthorizationsTestContext>(
                this.Context, 
                new PrincipalIdProvider(this.Context));

            this.AuthorizationsClient = new AuthorizationsClient<AuthorizationsTestContext>(
                this.Context, 
                new PrincipalIdProvider(this.Context), 
                new EntityFrameworkCore.Caching.AuthorizationsCacheProvider<AuthorizationsTestContext>(this.Context));
        }

        public AuthorizationsTestContext Context { get; private set; }

        public IAuthorizationsProvisioningClient AuthorizationsProvisioningClient { get; private set; }

        public IAuthorizationsClient AuthorizationsClient { get; private set; }

        public void Dispose()
        {
            this.Context.Database.CloseConnection();
            this.Context.Database.EnsureDeleted();
            this.Context.Dispose();
        }
    }
}
