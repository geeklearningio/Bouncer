namespace GeekLearning.Authorizations.Tests
{
    using System;
    using GeekLearning.Authorizations.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Data.Sqlite;
    using Model;

    public sealed class AuthorizationsFixture : IDisposable
    {
        public AuthorizationsTestContext Context { get; private set; }

        public IAuthorizationsProvisioningClient AuthorizationsProvisioningClient =>
            new AuthorizationsProvisioningClient<AuthorizationsTestContext>(Context, new PrincipalIdProvider(Context));

        public IAuthorizationsClient AuthorizationsClient { get; private set; }            

        public AuthorizationsFixture()
        {
            var builder = new DbContextOptionsBuilder<AuthorizationsTestContext>();

            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);

            builder.UseSqlite(connection);            

            Context = new AuthorizationsTestContext(builder.Options);

            Context.Database.OpenConnection();
            Context.Database.EnsureCreated();
            
            Context.Seed();

            this.AuthorizationsClient = new AuthorizationsClient<AuthorizationsTestContext>(Context, new PrincipalIdProvider(Context));
        }

        public AuthorizationsFixture(RightsResult rightsResult) : this()
        {
            this.AuthorizationsClient = new AuthorizationsTestClient(rightsResult);
        }

        public void Dispose()
        {
            this.Context.Dispose();
        }
    }
}
