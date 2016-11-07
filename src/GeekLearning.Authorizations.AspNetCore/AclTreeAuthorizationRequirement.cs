namespace GeekLearning.Authorizations.AspNetCore
{
    public class AclTreeAuthorizationRequirement : AclAuthorizationRequirement
    {
        public AclTreeAuthorizationRequirement(string right, string scope) : base(right, scope)
        { 
        }
    }
}
