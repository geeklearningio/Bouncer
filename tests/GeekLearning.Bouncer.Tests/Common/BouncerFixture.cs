namespace GeekLearning.Bouncer.Tests
{
    using Bouncer.EntityFrameworkCore.Queries;
    using EntityFrameworkCore;
    using GeekLearning.Bouncer.EntityFrameworkCore.Data;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public sealed class BouncerFixture : IDisposable
    {
        private readonly ServiceCollection serviceCollection;
        private readonly ServiceProvider serviceProvider;

        public BouncerFixture()
        {
            this.serviceCollection = new ServiceCollection();
            this.serviceCollection.AddScoped(s => this.AuthorizationsClient);
            this.serviceCollection.AddScoped(s => this.AuthorizationsManager);

            this.InitializeTestDatabase();

            var getParentGroupsIdQuery = new GetParentGroupsIdQuery(this.Context);
            this.AuthorizationsClient = new AuthorizationsClient(
                this.Context,
                new PrincipalIdProvider(this.Context.CurrentUserId),
                new GetScopeRightsQuery(
                    this.Context,
                    new EntityFrameworkCore.Caching.AuthorizationsCacheProvider(this.Context),
                    getParentGroupsIdQuery),
                getParentGroupsIdQuery);

            this.serviceProvider = this.serviceCollection.BuildServiceProvider();

            this.AuthorizationsManager = new AuthorizationsManager(
                this.Context,
                new PrincipalIdProvider(this.Context.CurrentUserId),
                this.serviceProvider);
        }
        
        public BouncerTestContext Context { get; private set; }
        
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
            var builder = new DbContextOptionsBuilder<BouncerContext>();

            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connection = new SqliteConnection(connectionStringBuilder.ToString());

            connection.Open();

            builder.UseSqlite(connection);

            this.Context = new BouncerTestContext(builder.Options);
            this.Context.Database.EnsureCreated();
            this.Context.Seed();

            this.serviceCollection.AddScoped(s => this.Context);
        }
    }
}
