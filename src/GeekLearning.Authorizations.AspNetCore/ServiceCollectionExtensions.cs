using Microsoft.AspNetCore.Authorization;
namespace GeekLearning.Authorizations.AspNetCore
{
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAclAuthorizationHandlers(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IAuthorizationHandler, AclTreeAuthorizationHandler>();
            serviceCollection.AddSingleton<IAuthorizationHandler, AclDirectAuthorizationHandler>();

            return serviceCollection;
        }
    }
}
