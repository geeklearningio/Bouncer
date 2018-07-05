namespace Bouncer.AspNetCore
{
    using Microsoft.AspNetCore.Authorization;

    public static class AclAuthorizationPolicyBuilderExtensions
    {
        public static AuthorizationPolicyBuilder RequireRightOnScope(this AuthorizationPolicyBuilder builder, string rightName, string scopeName)
        {
            return builder.AddRequirements(new AclDirectAuthorizationRequirement(rightName, scopeName));
        }

        public static AuthorizationPolicyBuilder RequireRightUnderScope(this AuthorizationPolicyBuilder builder, string rightName, string scopeName)
        {
            return builder.AddRequirements(new AclTreeAuthorizationRequirement(rightName, scopeName)); 
        }
    }
}
