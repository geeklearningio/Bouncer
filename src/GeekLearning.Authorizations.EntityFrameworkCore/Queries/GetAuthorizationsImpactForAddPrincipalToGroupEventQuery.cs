namespace GeekLearning.Authorizations.EntityFrameworkCore.Queries
{
    using GeekLearning.Authorizations.Events.Model;
    using GeekLearning.Authorizations.Events.Queries;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;

    public class GetAuthorizationsImpactForAddPrincipalToGroupEventQuery<TContext> : IGetImpactForAuthorizationEventQuery<AddPrincipalToGroup>
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
            var groupId = await this.context.Groups()
                .Where(g => g.Name == authorizationsEvent.GroupName)
                .Select(g => g.Id)
                .FirstOrDefaultAsync();
            var groupUpStream = await this.authorizationsClient.GetGroupParentLinkAsync(groupId);
            groupUpStream.Add(groupId);

            var impactedScopeNames = await this.context
                .Authorizations()
                .Join(groupUpStream, a => a.PrincipalId, pId => pId, (a, pId) => a.ScopeId)
                .Join(this.context.Scopes(), pId => pId, s => s.Id, (pId, s) => s.Name)
                .ToListAsync();

            var groupMembersIds = await this.authorizationsClient.GetGroupMembersAsync(authorizationsEvent.PrincipalId);
            groupMembersIds.Add(authorizationsEvent.PrincipalId);
            var groupIds = await this.context
                .Groups()
                .Join(groupMembersIds, g => g.Id, pId => pId, (g, pId) => pId)
                .ToListAsync();

            var impactedUserIds = groupMembersIds.Except(groupIds).ToList();

            return new AuthorizationsImpact
            {
                ScopeNames = impactedScopeNames,
                UserIds = impactedUserIds
            };
        }
    }
}
