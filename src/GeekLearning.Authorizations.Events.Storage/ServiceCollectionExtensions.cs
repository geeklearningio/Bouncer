namespace GeekLearning.Authorizations.Events.Storage
{
    using GeekLearning.Authorizations.Event;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAclEventsStorage(this IServiceCollection services)
        {
            services.AddScoped<IEventQueuer, StorageEventQueuer>();

            return services;
        }
    }
}
