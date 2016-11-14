namespace GeekLearning.Authorizations.Testing
{
    using GeekLearning.Authorizations.Model;

    public class UserRightsProviderService
    {
        public RightsResult CurrentRights { get; set; } = new RightsResult();
    }
}
