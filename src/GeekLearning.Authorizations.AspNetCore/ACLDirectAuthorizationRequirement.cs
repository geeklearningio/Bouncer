namespace GeekLearning.Authorizations.AspNetCore
{
    public class AclDirectAuthorizationRequirement : AclAuthorizationRequirement
    {
        public AclDirectAuthorizationRequirement(string right, string scope) : base(right, scope)
        {
        }
    }
}
