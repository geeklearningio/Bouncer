namespace GeekLearning.Authorizations.Events.Storage
{
    using GeekLearning.Authorizations.Events.Model;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;
    using System.Threading.Tasks;

    public class AuthorizationsImpactProcessor : IAuthorizationsImpactProcessor
    {
        private readonly CloudStorageAccount cloudStorageAccount;
        private readonly IAuthorizationsClient authorizationsClient;

        public AuthorizationsImpactProcessor(CloudStorageAccount cloudStorageAccount, IAuthorizationsClient authorizationsClient)
        {
            this.cloudStorageAccount = cloudStorageAccount;
            this.authorizationsClient = authorizationsClient;
        }

        public async Task StoreAuthorizationsImpactAsync(AuthorizationsImpact authorizationsImpact)
        {
            CloudBlobClient blobClient = this.cloudStorageAccount.CreateCloudBlobClient();
            foreach (var impactedUserId in authorizationsImpact.UserIds)
            {
                CloudBlobContainer container = blobClient.GetContainerReference($"authorizations-{impactedUserId}");
                await container.CreateIfNotExistsAsync();

                foreach (var impactedScopeName in authorizationsImpact.ScopeNames)
                {
                    var blob = container.GetBlockBlobReference(impactedScopeName);

                    var rightsOnScopeWithChildren = await this.authorizationsClient.GetRightsAsync(impactedScopeName, withChildren: true);

                    await blob.UploadTextAsync(JsonConvert.SerializeObject(rightsOnScopeWithChildren));
                }
            }
        }
    }
}
