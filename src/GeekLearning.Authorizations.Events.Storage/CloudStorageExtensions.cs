namespace Microsoft.WindowsAzure.Storage
{
    using System.Threading.Tasks;

    public static class CloudStorageExtensions
    {
        public async static Task EnsureAuthorizationsQueueIsCreatedAsync(this CloudStorageAccount storageAccount, string queueName)
        {
            var queueClient = storageAccount.CreateCloudQueueClient();

            var queueReference = queueClient.GetQueueReference(queueName);
            await queueReference.CreateIfNotExistsAsync();
        }
    }
}
