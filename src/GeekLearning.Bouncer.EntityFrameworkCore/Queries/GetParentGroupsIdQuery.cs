namespace GeekLearning.Bouncer.EntityFrameworkCore.Queries
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class GetParentGroupsIdQuery<TContext> : IGetParentGroupsIdQuery where TContext : DbContext
    {
        private readonly TContext context;

        public GetParentGroupsIdQuery(TContext context)
        {
            this.context = context;
        }

        public async Task<IList<Guid>> ExecuteAsync(params Guid[] principalsId)
        {
            List<Guid> groupIds = new List<Guid>();
            var groupParentsId = await this.context.Memberships()
                .Where(m => principalsId.Contains(m.PrincipalId))
                .Select(m => m.GroupId)
                .ToListAsync();
            groupIds.AddRange(groupParentsId);

            if (groupParentsId.Count > 0)
            {
                groupIds.AddRange(await this.ExecuteAsync(groupParentsId.ToArray()));
            }

            return groupIds;
        }
    }
}
