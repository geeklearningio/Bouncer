namespace GeekLearning.Authorizations.Events.Storage.Receivers
{
    using GeekLearning.Authorizations.Events.Model;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System.Threading.Tasks;

    public class AddPrincipalToGroupEventReceiver : IEventReceiver<AddPrincipalToGroup>
    {
        private readonly AddPrincipalToGroup authorizationsEvent;
        private readonly CloudStorageAccount cloudStorageAccount;

        public AddPrincipalToGroupEventReceiver(CloudStorageAccount cloudStorageAccount)
        {
            this.cloudStorageAccount = cloudStorageAccount;
        }

        public Task ReceiveAsync(AddPrincipalToGroup authorizationsEvent)
        {
            CloudBlobClient cloudBlobClient = this.cloudStorageAccount.CreateCloudBlobClient();



            return Task.CompletedTask;
        }
    }
}
