namespace GeekLearning.Authorizations.AspNetCore
{
    public class AclTreeAuthorizationRequirement : AclAuthorizationRequirement
    {
        public AclTreeAuthorizationRequirement(string rightName, string scopeName) : base(rightName, scopeName)
        { 
        }
    }
}
