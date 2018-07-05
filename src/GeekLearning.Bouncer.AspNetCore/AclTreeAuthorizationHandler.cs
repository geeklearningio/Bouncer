namespace GeekLearning.Bouncer.AspNetCore
{
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;

    public class AclTreeAuthorizationHandler : AuthorizationHandler<AclTreeAuthorizationRequirement>
    {
        private readonly IAuthorizationsClient authorizationClient;

        public AclTreeAuthorizationHandler(IAuthorizationsClient authorizationClient)
        {
            this.authorizationClient = authorizationClient; 
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AclTreeAuthorizationRequirement requirement)
        {
            var results = await this.authorizationClient.GetRightsAsync(requirement.ScopeName, withChildren: true);
            if (results.HasRightUnderScope(requirement.RightName, requirement.ScopeName))
            {
                context.Succeed(requirement);
            }
        }
    }
}
