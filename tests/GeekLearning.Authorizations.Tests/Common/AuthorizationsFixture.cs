namespace GeekLearning.Authorizations.Tests
{
    using EntityFrameworkCore;
    using GeekLearning.Authorizations.Event;
    using GeekLearning.Authorizations.Events;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public sealed class AuthorizationsFixture : IDisposable
    {
        private readonly ServiceCollection serviceCollection;
        private readonly ServiceProvider serviceProvider;

        public AuthorizationsFixture()
        {
            this.serviceCollection = new ServiceCollection();
            this.serviceCollection.AddScoped(s => this.AuthorizationsClient);
            this.serviceCollection.AddScoped(s => this.AuthorizationsProvisioningClient);
            this.serviceCollection.AddScoped(s => this.AuthorizationsEventQueuer);
            this.serviceCollection.AddScoped<IAuthorizationsImpactClient>(s => this.AuthorizationsImpactClient);
            this.serviceCollection.AddAclEvents<AuthorizationsTestContext>();

            this.InitializeTestDatabase();
            
            this.AuthorizationsClient = new AuthorizationsClient<AuthorizationsTestContext>(
                this.Context,
                new PrincipalIdProvider(this.Context),
                new EntityFrameworkCore.Caching.AuthorizationsCacheProvider<AuthorizationsTestContext>(this.Context));

            this.AuthorizationsImpactClient = new AuthorizationsTestImpactClient(this.AuthorizationsClient);

            this.serviceProvider = this.serviceCollection.BuildServiceProvider();

            this.AuthorizationsEventQueuer = new AuthorizationsTestEventQueuer(
                new AuthorizationsEventReceiver(this.serviceProvider, this.AuthorizationsImpactClient));

            this.AuthorizationsProvisioningClient = new AuthorizationsProvisioningClient<AuthorizationsTestContext>(
                this.Context,
                new PrincipalIdProvider(this.Context),
                this.serviceProvider);
        }

        public AuthorizationsTestContext Context { get; private set; }

        public AuthorizationsTestImpactClient AuthorizationsImpactClient { get; private set; }

        public IEventQueuer AuthorizationsEventQueuer { get; private set; }

        public IAuthorizationsProvisioningClient AuthorizationsProvisioningClient { get; private set; }

        public IAuthorizationsClient AuthorizationsClient { get; private set; }

        public void Dispose()
        {
            this.Context.Database.CloseConnection();
            this.Context.Database.EnsureDeleted();
            this.Context.Dispose();
        }

        private void InitializeTestDatabase()
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

            this.serviceCollection.AddScoped(s => this.Context);
        }
    }
}
