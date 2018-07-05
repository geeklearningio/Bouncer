namespace GeekLearning.Bouncer.EntityFrameworkCore.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IGetParentGroupsIdQuery
    {
        Task<IList<Guid>> ExecuteAsync(params Guid[] principalsId);
    }
}