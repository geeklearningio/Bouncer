namespace Bouncer.EntityFrameworkCore.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAuthorizationsCacheProvider
    {
        Task<IDictionary<Guid, Role>> GetRolesAsync();

        Task<ScopesCache> GetScopesCacheAsync();

        Task<IDictionary<TKey, Scope>> GetScopesAsync<TKey>(Func<Scope, TKey> keySelector);
    }
}
