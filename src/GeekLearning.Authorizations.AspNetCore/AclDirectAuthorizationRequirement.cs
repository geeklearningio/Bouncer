namespace GeekLearning.Authorizations.AspNetCore
{
    public class AclDirectAuthorizationRequirement : AclAuthorizationRequirement
    {
        public AclDirectAuthorizationRequirement(string rightName, string scopeName) : base(rightName, scopeName)
        { 
        }
    }
}
