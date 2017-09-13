namespace GeekLearning.Authorizations.Events.Storage
{
    using GeekLearning.Authorizations.Event;
    using GeekLearning.Authorizations.Events.Model;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class StorageEventQueuer : IEventQueuer
    {
        private readonly StorageOptions storageOptions;
        private readonly CloudStorageAccount storageAccount;
        private readonly Queue<EventBase> queue = new Queue<EventBase>();

        public StorageEventQueuer(StorageOptions storageOptions)
        {
            this.storageOptions = storageOptions;
            this.storageAccount = CloudStorageAccount.Parse(storageOptions.ConnectionString);
        }

        public void QueueEvent<TEvent>(TEvent authorizationsEvent) where TEvent : EventBase
        {
            if (!this.queue.Any(e => e == authorizationsEvent))
            {
                this.queue.Enqueue(authorizationsEvent);
            }
        }

        public async Task CommitAsync()
        {
            await this.storageAccount.EnsureAuthorizationsQueueIsCreatedAsync(storageOptions.QueueName);

            CloudQueueClient queueClient = this.storageAccount.CreateCloudQueueClient();
            CloudQueue cloudQueue = queueClient.GetQueueReference(this.storageOptions.QueueName);

            List<Task> addMessagesTaskList = new List<Task>();
            foreach (EventBase authorizationEvent in this.queue)
            {
                addMessagesTaskList.Add(
                    cloudQueue.AddMessageAsync(
                        new CloudQueueMessage(JsonConvert.SerializeObject(authorizationEvent))));                
            }

            this.queue.Clear();

            await Task.WhenAll(addMessagesTaskList);
        }
    }
}
