namespace GeekLearning.Authorizations
{
    using Model;
    using System.Threading.Tasks;

    public interface IAuthorizationsClient
    {
        /// <summary>
        /// Configure the client the get rights for the specified scope.
        /// </summary>
        /// <param name="scopeKey"></param>
        /// <param name="withChildren"></param>
        /// <returns></returns>
        Task<RightsResult> GetRightsAsync(string scopeKey, bool withChildren = false);

        Task<bool> HasRightAsync(string rightKey, string scopeKey);
    }
}
