namespace Bouncer.EntityFrameworkCore
{
    using Bouncer.EntityFrameworkCore.Queries;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAclDbContext<TContext, TPrincipalIdProvider>(this IServiceCollection services)
            where TContext : DbContext
            where TPrincipalIdProvider : class, IPrincipalIdProvider
        {
            services.AddScoped<IPrincipalIdProvider, TPrincipalIdProvider>();
            services.AddScoped<IAuthorizationsClient, AuthorizationsClient<TContext>>();
            services.AddScoped<IAuthorizationsManager, AuthorizationsManager<TContext>>();
            services.AddScoped<IGetScopeRightsQuery, GetScopeRightsQuery<TContext>>();
            services.AddScoped<IGetParentGroupsIdQuery, GetParentGroupsIdQuery<TContext>>();
            services.AddScoped<Caching.IAuthorizationsCacheProvider, Caching.AuthorizationsCacheProvider<TContext>>();

            return services;
        }
    }
}
