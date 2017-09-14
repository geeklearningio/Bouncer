namespace GeekLearning.Authorizations.Events.Storage.Receivers
{
    using GeekLearning.Authorizations.Events.Model;
    using GeekLearning.Authorizations.Events.Queries;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System.Threading.Tasks;

    public class AddPrincipalToGroupEventReceiver : IEventReceiver<AddPrincipalToGroup>
    {
        private readonly AddPrincipalToGroup authorizationsEvent;
        private readonly CloudStorageAccount cloudStorageAccount;
        private readonly IGetImpactedScopesForAuthorizationEventQuery<AddPrincipalToGroup> getImpactedScopesForAddPrincipalToGroupEventQuery;

        public AddPrincipalToGroupEventReceiver(CloudStorageAccount cloudStorageAccount, IGetImpactedScopesForAuthorizationEventQuery<AddPrincipalToGroup> getImpactedScopesForAddPrincipalToGroupEventQuery)
        {
            this.cloudStorageAccount = cloudStorageAccount;
            this.getImpactedScopesForAddPrincipalToGroupEventQuery = getImpactedScopesForAddPrincipalToGroupEventQuery;
        }

        public async Task ReceiveAsync(AddPrincipalToGroup authorizationsEvent)
        {
            CloudBlobClient cloudBlobClient = this.cloudStorageAccount.CreateCloudBlobClient();

            var scopeIds = await this.getImpactedScopesForAddPrincipalToGroupEventQuery.ExecuteAsync(authorizationsEvent);            
        }
    }
}
