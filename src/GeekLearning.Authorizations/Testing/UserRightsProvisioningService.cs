namespace GeekLearning.Authorizations.Testing
{
    using GeekLearning.Authorizations.Model;

    public class UserRightsProvisioningService : IUserRightsProvisioningService
    {
        public RightsResult CurrentRights { get; set; }
    }
}
