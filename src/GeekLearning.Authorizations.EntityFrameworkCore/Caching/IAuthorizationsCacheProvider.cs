namespace GeekLearning.Authorizations.EntityFrameworkCore.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAuthorizationsCacheProvider
    {
        Task<IDictionary<Guid, Role>> GetRolesAsync();

        Task<IDictionary<Guid, Scope>> GetScopesAsync();
    }
}
