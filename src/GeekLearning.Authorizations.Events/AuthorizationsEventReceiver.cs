namespace GeekLearning.Authorizations.Events
{
    using GeekLearning.Authorizations.Events.Model;
    using GeekLearning.Authorizations.Events.Queries;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    public class AuthorizationsEventReceiver
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IAuthorizationsImpactClient authorizationsAnalyzer;

        public AuthorizationsEventReceiver(IServiceProvider serviceProvider, IAuthorizationsImpactClient authorizationsImpactClient)
        {
            this.serviceProvider = serviceProvider;
            this.authorizationsAnalyzer = authorizationsImpactClient;
        }

        public async Task ReceiveAsync<TEvent>(TEvent authorizationsEvent) where TEvent : EventBase
        {
            var queryType = typeof(IGetImpactForAuthorizationEventQuery<>)
                .MakeGenericType(new Type[] { authorizationsEvent.GetType() });

            var getImpactForAuthorizationEventQuery = this.serviceProvider.GetRequiredService(queryType);
            
            var task = (Task<AuthorizationsImpact>)queryType.GetMethod("ExecuteAsync")
                .Invoke(getImpactForAuthorizationEventQuery, new object[] { authorizationsEvent });
            
            var authorizationsImpact = await task.ConfigureAwait(false);

            await this.authorizationsAnalyzer.StoreAuthorizationsImpactAsync(authorizationsImpact);
        }
    } 
}
