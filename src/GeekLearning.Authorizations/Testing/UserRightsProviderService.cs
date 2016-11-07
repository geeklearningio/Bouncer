namespace GeekLearning.Authorizations.Testing
{
    using GeekLearning.Authorizations.Model;

    public class UserRightsProviderService : IUserRightsProviderService
    {
        public RightsResult CurrentRights { get; set; }
    }
}
