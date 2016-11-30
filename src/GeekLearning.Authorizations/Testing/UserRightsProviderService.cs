namespace GeekLearning.Authorizations.Testing
{
    using Model;

    public class UserRightsProviderService
    {
        public RightsResult CurrentRights { get; set; } = new RightsResult();
    }
}
