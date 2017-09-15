namespace GeekLearning.Authorizations.Events
{
    using GeekLearning.Authorizations.Events.Model;
    using GeekLearning.Authorizations.Events.Queries;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading.Tasks;

    public class AuthorizationsEventReceiver
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IAuthorizationsImpactProcessor authorizationsAnalyzer;

        public AuthorizationsEventReceiver(IServiceProvider serviceProvider, IAuthorizationsImpactProcessor authorizationsAnalyzer)
        {
            this.serviceProvider = serviceProvider;
            this.authorizationsAnalyzer = authorizationsAnalyzer;
        }

        public async Task ReceiveAsync<TEvent>(TEvent authorizationsEvent) where TEvent : EventBase
        {
            var getImpactForAuthorizationEventQuery = this.serviceProvider.GetRequiredService<IGetImpactForAuthorizationEventQuery<TEvent>>();
            
            var authorizationsImpact = await getImpactForAuthorizationEventQuery.ExecuteAsync(authorizationsEvent);

            await this.authorizationsAnalyzer.StoreAuthorizationsImpactAsync(authorizationsImpact);
        }
    } 
}
