namespace GeekLearning.Authorizations.AspNetCore
{
    using Microsoft.AspNetCore.Authorization;

    public static class AclAuthorizationPolicyBuilderExtensions
    {
        public static AuthorizationPolicyBuilder RequireRightOnScope(this AuthorizationPolicyBuilder builder, string rightKey, string scopeKey)
        {
            return builder.AddRequirements(new AclDirectAuthorizationRequirement(rightKey, scopeKey));
        }

        public static AuthorizationPolicyBuilder RequireRightUnderScope(this AuthorizationPolicyBuilder builder, string rightKey, string scopeKey)
        {
            return builder.AddRequirements(new AclTreeAuthorizationRequirement(rightKey, scopeKey));
        }
    }
}
