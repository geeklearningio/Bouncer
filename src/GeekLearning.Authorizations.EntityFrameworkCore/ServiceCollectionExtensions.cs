namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using GeekLearning.Authorizations.EntityFrameworkCore.Queries;
    using GeekLearning.Authorizations.Events.Model;
    using GeekLearning.Authorizations.Events.Queries;
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
            services.AddScoped<IAuthorizationsProvisioningClient, AuthorizationsProvisioningClient<TContext>>();
            services.AddScoped<Caching.IAuthorizationsCacheProvider, Caching.AuthorizationsCacheProvider<TContext>>();

            services.AddScoped<IGetImpactForAuthorizationEventQuery<AddPrincipalToGroup>, GetAuthorizationsImpactForAddPrincipalToGroupEventQuery<TContext>>();

            return services;
        }
    }
}
