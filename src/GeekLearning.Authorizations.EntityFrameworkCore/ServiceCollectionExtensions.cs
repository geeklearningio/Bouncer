namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddACL<TContext>(this IServiceCollection services)
            where TContext : DbContext 
        {
            services.AddTransient<IAuthorizationsClient, AuthorizationsClient<TContext>>();
            services.AddTransient<IAuthorizationsProvisioningClient, AuthorizationsProvisioningClient<TContext>>();

            return services;
        }
    }
}
