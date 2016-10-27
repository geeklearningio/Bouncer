namespace GeekLearning.Authorizations.AspNetCore
{
    using Microsoft.AspNetCore.Authorization;

    public abstract class AclAuthorizationRequirement : IAuthorizationRequirement
    {
        public AclAuthorizationRequirement(string right, string scope)
        {
            this.Right = right;
            this.Scope = scope;
        }

        public string Right { get; set; }

        public string Scope { get; set; }
    }
}
