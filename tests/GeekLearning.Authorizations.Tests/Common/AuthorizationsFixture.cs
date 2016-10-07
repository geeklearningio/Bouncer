namespace GeekLearning.Authorizations.Tests
{
    using System;
    using GeekLearning.Authorizations.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Data.Sqlite;

    public sealed class AuthorizationsFixture : IDisposable
    {
        public AuthorizationsTestContext Context { get; private set; }

        public IAuthorizationsProvisioningClient AuthorizationsProvisioningClient =>
            new AuthorizationsProvisioningClient<AuthorizationsTestContext>(Context, Context.CurrentUserId);

        public IAuthorizationsClient AuthorizationsClient =>
            new AuthorizationsClient<AuthorizationsTestContext>(Context, Context.CurrentUserId);

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
        }

        public void Dispose()
        {
            this.Context.Dispose();
        }
    }
}
