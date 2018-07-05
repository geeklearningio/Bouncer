namespace GeekLearning.Bouncer.Tests
{
    using EntityFrameworkCore;
    using Bouncer.EntityFrameworkCore.Queries;
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
            this.serviceCollection.AddScoped(s => this.AuthorizationsManager);

            this.InitializeTestDatabase();

            var getParentGroupsIdQuery = new GetParentGroupsIdQuery<AuthorizationsTestContext>(this.Context);
            this.AuthorizationsClient = new AuthorizationsClient<AuthorizationsTestContext>(
                this.Context,
                new PrincipalIdProvider(this.Context),
                new GetScopeRightsQuery<AuthorizationsTestContext>(
                    this.Context, 
                    new EntityFrameworkCore.Caching.AuthorizationsCacheProvider<AuthorizationsTestContext>(this.Context),
                    getParentGroupsIdQuery),
                getParentGroupsIdQuery);

            this.serviceProvider = this.serviceCollection.BuildServiceProvider();

            this.AuthorizationsManager = new AuthorizationsManager<AuthorizationsTestContext>(
                this.Context,
                new PrincipalIdProvider(this.Context),
                this.serviceProvider);
        }

        public AuthorizationsTestContext Context { get; private set; }
        
        public IAuthorizationsManager AuthorizationsManager { get; private set; }

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
