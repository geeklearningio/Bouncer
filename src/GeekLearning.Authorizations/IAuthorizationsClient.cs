namespace GeekLearning.Authorizations
{
    using Model;
    using System.Threading.Tasks;

    public interface IAuthorizationsClient
    {
        /// <summary>
        /// Configure the client the get rights for the specified scope. Call the ExecuteAsync method to get the result.
        /// </summary>
        /// <param name="scopeKey"></param>
        /// <returns></returns>
        IAuthorizationsClient GetRights(string scopeKey);

        /// <summary>
        /// Configure the client the get rights for the specified scope with its children. Call the ExecuteAsync method to get the result.
        /// </summary>
        /// <returns></returns>
        IAuthorizationsClient WithChildren();

        /// <summary>
        /// Evaluates the client configuration to get the right results.
        /// </summary>
        /// <returns></returns>
        Task<RightsResult> ExecuteAsync();
        
        Task<bool> HasRightAsync(string rightKey, string scopeKey);
    }
}
