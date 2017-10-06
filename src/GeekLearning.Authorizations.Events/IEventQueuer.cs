namespace GeekLearning.Authorizations.Event
{
    using GeekLearning.Authorizations.Events.Model;
    using System.Threading.Tasks;

    public interface IEventQueuer
    {
        void QueueEvent<TEvent>(TEvent authorizationsEvent) where TEvent : EventBase;

        Task CommitAsync();
    }
}
