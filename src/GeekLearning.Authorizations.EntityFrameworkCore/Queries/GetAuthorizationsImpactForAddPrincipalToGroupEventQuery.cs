namespace GeekLearning.Authorizations.EntityFrameworkCore.Queries
{
    using GeekLearning.Authorizations.Events.Model;
    using GeekLearning.Authorizations.Events.Queries;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;

    public class GetAuthorizationsImpactForAddPrincipalToGroupEventQuery<TContext>: IGetImpactForAuthorizationEventQuery<AddPrincipalToGroup>
        where TContext : DbContext
    {
        private readonly TContext context;
        private readonly IAuthorizationsClient authorizationsClient;

        public GetAuthorizationsImpactForAddPrincipalToGroupEventQuery(TContext context, IAuthorizationsClient authorizationsClient)
        {
            this.context = context;
            this.authorizationsClient = authorizationsClient;
        }

        public async Task<AuthorizationsImpact> ExecuteAsync(AddPrincipalToGroup authorizationsEvent)
        {
            var principalIds = await this.authorizationsClient.GetMembershipsAsync(authorizationsEvent.PrincipalId);
            principalIds.Add(authorizationsEvent.PrincipalId);
            
            var impactedScopeNames = await this.context
                .Authorizations()
                .Join(principalIds, a => a.PrincipalId, pId => pId, (a, pId) => a.ScopeId)
                .Join(this.context.Scopes(), pId => pId, s => s.Id, (pId, s) => s.Name)
                .ToListAsync();

            var groupIds = await this.context
                .Groups()
                .Join(principalIds, g => g.Id, pId => pId, (g, pId) => pId)
                .ToListAsync();

            var impactedUserIds = principalIds.Except(groupIds).ToList();

            return new AuthorizationsImpact
            {
                ScopeNames = impactedScopeNames,
                UserIds = impactedUserIds
            };
        }
    }
}
