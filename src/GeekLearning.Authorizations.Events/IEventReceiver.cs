namespace GeekLearning.Authorizations.Events
{
    using GeekLearning.Authorizations.Events.Model;
    using System.Threading.Tasks;

    public interface IEventReceiver<TEvent> where TEvent : EventBase
    {
        Task ReceiveAsync(TEvent authorizationsEvent);
    }
}
