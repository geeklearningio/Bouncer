namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDbContextACL<TContext, TPrincipalIdProvider>(this IServiceCollection services)
            where TContext : DbContext
            where TPrincipalIdProvider : class, IPrincipalIdProvider
        {
            services.AddTransient<IPrincipalIdProvider, TPrincipalIdProvider>();
            services.AddTransient<IAuthorizationsClient, AuthorizationsClient<TContext>>();
            services.AddTransient<IAuthorizationsProvisioningClient, AuthorizationsProvisioningClient<TContext>>();
        
            return services;
        }
    }
}
