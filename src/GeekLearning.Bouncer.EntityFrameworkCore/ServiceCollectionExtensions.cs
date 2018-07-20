namespace GeekLearning.Bouncer.EntityFrameworkCore
{
    using Bouncer.EntityFrameworkCore.Configuration;
    using Bouncer.EntityFrameworkCore.Queries;
    using GeekLearning.Bouncer.EntityFrameworkCore.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBouncer<TPrincipalIdProvider>(this IServiceCollection services, IConfigurationSection configurationSection)
            where TPrincipalIdProvider : class, IPrincipalIdProvider
        {
            services.AddScoped<IPrincipalIdProvider, TPrincipalIdProvider>();
            services.AddScoped<IAuthorizationsClient, AuthorizationsClient>();
            services.AddScoped<IAuthorizationsManager, AuthorizationsManager>();
            services.AddScoped<IGetScopeRightsQuery, GetScopeRightsQuery>();
            services.AddScoped<IGetParentGroupsIdQuery, GetParentGroupsIdQuery>();
            services.AddScoped<Caching.IAuthorizationsCacheProvider, Caching.AuthorizationsCacheProvider>();

            services.AddDbContextPool<BouncerContext>((sp, optionsBuilder) =>
            {
                if (!Enum.TryParse(configurationSection["Provider"], out BouncerSupportedProviders selectedBouncerProvider))
                {
                    throw new InvalidOperationException("No valid provider was specified, please check your configuration");
                }

                optionsBuilder.UseInternalServiceProvider(sp);

                if (selectedBouncerProvider == BouncerSupportedProviders.SqlServer)
                {
                    optionsBuilder.UseSqlServer(
                        GetConnectionString(configurationSection),
                        options => options.EnableRetryOnFailure());
                }
                else if (selectedBouncerProvider == BouncerSupportedProviders.Sqlite)
                {
                    // Data Source=:memory:
                    optionsBuilder.UseSqlite(GetConnectionString(configurationSection));
                }
            });

            return services;
        }

        private static string GetConnectionString(IConfigurationSection configurationSection)
        {
            var connectionString = configurationSection["ConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("No valid connection string was specified, please check your configuration");
            }

            return connectionString;
        }
    }
}
