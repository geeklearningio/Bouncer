namespace GeekLearning.Authorizations.EntityFrameworkCore.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IGetParentGroupsIdQuery
    {
        Task<IList<Guid>> ExecuteAsync(IEnumerable<Guid> principalsId);
    }
}