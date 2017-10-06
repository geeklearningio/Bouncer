namespace GeekLearning.Authorizations.Events.Queries
{
    using GeekLearning.Authorizations.Events.Model;
    using System.Threading.Tasks;

    public interface IGetImpactForAuthorizationEventQuery<TEvent> where TEvent : EventBase
    {
        Task<AuthorizationsImpact> ExecuteAsync(TEvent authorizationsEvent);
    }
}
