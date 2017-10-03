namespace GeekLearning.Authorizations.Tests
{
    using GeekLearning.Authorizations.Event;
    using GeekLearning.Authorizations.Events;
    using GeekLearning.Authorizations.Events.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class AuthorizationsTestEventQueuer : IEventQueuer
    {
        private readonly Queue<EventBase> authorizationsEventQueue = new Queue<EventBase>();
        private readonly AuthorizationsEventReceiver authorizationsEventReceiver;

        public AuthorizationsTestEventQueuer(AuthorizationsEventReceiver authorizationsEventReceiver)
        {
            this.authorizationsEventReceiver = authorizationsEventReceiver;
        }

        public async Task CommitAsync()
        {
            foreach (var authorizationEvent in this.authorizationsEventQueue)
            {
                await this.authorizationsEventReceiver.ReceiveAsync(authorizationEvent);
            }
        }

        public void QueueEvent<TEvent>(TEvent authorizationsEvent) where TEvent : EventBase
        {
            if (!this.authorizationsEventQueue.Any(e => e == authorizationsEvent))
            {
                this.authorizationsEventQueue.Enqueue(authorizationsEvent);
            }
        }
    }
}
