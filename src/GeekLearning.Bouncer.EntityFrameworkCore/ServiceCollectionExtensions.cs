namespace GeekLearning.Bouncer.EntityFrameworkCore
{
    using Bouncer.EntityFrameworkCore.Queries;
    using Bouncer.EntityFrameworkCore.Configuration;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBouncer<TContext, TPrincipalIdProvider>(this IServiceCollection services, IConfigurationSection configurationSection)
            where TContext : DbContext
            where TPrincipalIdProvider : class, IPrincipalIdProvider
        {
            services.AddScoped<IPrincipalIdProvider, TPrincipalIdProvider>();
            services.AddScoped<IAuthorizationsClient, AuthorizationsClient<TContext>>();
            services.AddScoped<IAuthorizationsManager, AuthorizationsManager<TContext>>();
            services.AddScoped<IGetScopeRightsQuery, GetScopeRightsQuery<TContext>>();
            services.AddScoped<IGetParentGroupsIdQuery, GetParentGroupsIdQuery<TContext>>();
            services.AddScoped<Caching.IAuthorizationsCacheProvider, Caching.AuthorizationsCacheProvider<TContext>>();

            services.AddDbContextPool<BouncerContext>((sp, optionsBuilder) =>
            {
                if (Enum.TryParse(configurationSection["Provider"], out BouncerSupportedProviders selectedBouncerProvider))
                {
                    throw new InvalidOperationException("No valid provider was specified, please check your configuration");
                }

                optionsBuilder.UseInternalServiceProvider(sp);

                if (selectedBouncerProvider == BouncerSupportedProviders.SqlServer)
                {
                    var connectionString = configurationSection["ConnectionString"];
                    if (string.IsNullOrWhiteSpace(connectionString))
                    {
                        throw new InvalidOperationException("No valid connection string was specified, please check your configuration");
                    }

                    optionsBuilder.UseSqlServer(connectionString, options => options.EnableRetryOnFailure());
                }
            });

            return services;
        }
    }
}
