namespace GeekLearning.Authorizations.AspNetCore
{
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;

    public class AclDirectAuthorizationHandler : AuthorizationHandler<AclDirectAuthorizationRequirement>
    {
        private readonly IAuthorizationsClient authorizationClient;

        public AclDirectAuthorizationHandler(IAuthorizationsClient authorizationClient)
        {
            this.authorizationClient = authorizationClient; 
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AclDirectAuthorizationRequirement requirement)
        {
            if (await this.authorizationClient.HasRightAsync(requirement.Right, requirement.Scope))
            {
                context.Succeed(requirement);
            }
        }
    }
}
