namespace GeekLearning.Authorizations.Events
{
    using GeekLearning.Authorizations.Events.Model;
    using System.Threading.Tasks;

    public interface IAuthorizationsImpactProcessor
    {
        Task StoreAuthorizationsImpactAsync(AuthorizationsImpact authorizationsImpact);
    }
}
