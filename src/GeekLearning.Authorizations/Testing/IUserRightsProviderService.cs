using GeekLearning.Authorizations.Model;

namespace GeekLearning.Authorizations.Testing
{
    public interface IUserRightsProviderService
    {
        RightsResult CurrentRights { get; set; }
    }
}
