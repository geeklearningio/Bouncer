namespace GeekLearning.Bouncer.AspNetCore
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAclAuthorizationHandlers(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IAuthorizationHandler, AclTreeAuthorizationHandler>();
            serviceCollection.AddScoped<IAuthorizationHandler, AclDirectAuthorizationHandler>();
             
            return serviceCollection;
        }
    }
}
