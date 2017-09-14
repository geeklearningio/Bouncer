namespace GeekLearning.Authorizations.Events.Queries
{
    using GeekLearning.Authorizations.Events.Model;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IGetImpactedScopesForAuthorizationEventQuery<TEvent> where TEvent : EventBase
    {
        Task<IEnumerable<Guid>> ExecuteAsync(TEvent authorizationsEvent);
    }
}
