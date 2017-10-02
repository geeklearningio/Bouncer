namespace GeekLearning.Authorizations.Events
{
    using GeekLearning.Authorizations.Events.Model;
    using System.Threading.Tasks;

    public interface IAuthorizationsImpactClient
    {
        Task StoreAuthorizationsImpactAsync(AuthorizationsImpact authorizationsImpact);
    }
}
