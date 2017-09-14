namespace GeekLearning.Authorizations.EntityFrameworkCore.Queries
{
    using GeekLearning.Authorizations.Events.Model;
    using GeekLearning.Authorizations.Events.Queries;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class GetAuthorizationsImpactForAddPrincipalToGroupEventQuery<TContext>: IGetImpactedScopesForAuthorizationEventQuery<AddPrincipalToGroup>
        where TContext : DbContext
    {
        private readonly TContext context;

        public GetAuthorizationsImpactForAddPrincipalToGroupEventQuery(TContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<Guid>> ExecuteAsync(AddPrincipalToGroup authorizationsEvent)
        {
            List<Guid> principalIds = new List<Guid>() { authorizationsEvent.PrincipalId };
            await DetectMembershipsAsync(authorizationsEvent.PrincipalId, principalIds);            

            return await this.context
                .Authorizations()
                .Join(principalIds, a => a.PrincipalId, pId => pId, (a, pId) => pId)
                .ToListAsync();
        }

        private async Task DetectMembershipsAsync(Guid principalId, List<Guid> principalIds)
        {
            var groupMembers = await this.context.Memberships()
                .Where(m => m.GroupId == principalId)
                .ToListAsync();
            principalIds.AddRange(groupMembers.Select(gm => gm.PrincipalId));
            foreach (var groupMember in groupMembers)
            {
                await DetectMembershipsAsync(groupMember.PrincipalId, principalIds);
            }
        }
    }
}
