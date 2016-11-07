using GeekLearning.Authorizations.Model;

namespace GeekLearning.Authorizations.Testing
{
    public interface IUserRightsProvisioningService
    {
        RightsResult CurrentRights { get; set; }
    }
}
