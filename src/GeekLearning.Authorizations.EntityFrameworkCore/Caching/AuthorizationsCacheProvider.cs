namespace GeekLearning.Authorizations.EntityFrameworkCore.Caching
{
    using GeekLearning.Authorizations.EntityFrameworkCore.Exceptions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Memory;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class AuthorizationsCacheProvider<TContext> : IAuthorizationsCacheProvider where TContext : DbContext
    {
        private readonly TContext context;
        private readonly IMemoryCache memoryCache;

        public AuthorizationsCacheProvider(TContext context, IMemoryCache memoryCache = null)
        {
            this.context = context;
            this.memoryCache = memoryCache;
        }

        public async Task<IDictionary<Guid, Role>> GetRolesAsync()
        {
            var rolesCache = await this.GetOrCreateAsync(
                RolesCache.GetCacheKey(),
                () => this.QueryRolesAsync(),
                mmd => mmd.Roles);

            return rolesCache.Compute();
        }

        public async Task<IDictionary<Guid, Scope>> GetScopesAsync()
        {
            var scopesCache = await this.GetOrCreateAsync(
                ScopesCache.GetCacheKey(),
                () => this.QueryScopesAsync(),
                mmd => mmd.Scopes);

            return scopesCache.Compute();
        }

        private async Task<RolesCache> QueryRolesAsync()
        {
            var roles = await this.context.Roles()
                .Join(this.context.RoleRights(), r => r.Id, rr => rr.RoleId, (r, rr) => new { Role = r, RoleRights = rr })
                .Join(this.context.Rights(), j => j.RoleRights.RightId, r => r.Id, (j, r) => new { RoleId = j.Role.Id, RoleName = j.Role.Name, RightName = r.Name })
                .GroupBy(j => j.RoleId)
                .Select(g => new Role { Id = g.Key, Name = g.First().RoleName, Rights = g.Select(sg => sg.RightName).ToList() })
                .ToListAsync();

            return new RolesCache { Roles = roles };
        }

        private async Task<ScopesCache> QueryScopesAsync()
        {
            var dataScopes = await this.context.Scopes()
                                    .Select(s => new { s.Id, s.Name })
                                    .ToListAsync();

            var dataScopeHierarchies = await this.context.ScopeHierarchies()
                .Select(s => new { s.ParentId, s.ChildId })
                .ToListAsync();

            var groupByParent = dataScopeHierarchies
                .GroupBy(sh => sh.ParentId)
                .ToDictionary(g => g.Key, g => g.Select(sh => sh.ChildId).ToList());

            var groupByChild = dataScopeHierarchies
                .GroupBy(sh => sh.ChildId)
                .ToDictionary(g => g.Key, g => g.Select(sh => sh.ParentId).ToList());

            var scopes = dataScopes
                .Select(s => new Scope
                {
                    Id = s.Id,
                    Name = s.Name,
                    ChildIds = groupByParent.ContainsKey(s.Id) ? groupByParent[s.Id] : null,
                    ParentIds = groupByChild.ContainsKey(s.Id) ? groupByChild[s.Id] : null,
                })
                .ToDictionary(ds => ds.Id, ds => ds);
            
            this.ValidateScopeModel(scopes);

            return new ScopesCache { Scopes = scopes.Values };
        }

        private void ValidateScopeModel(IDictionary<Guid, Scope> scopes)
        {
            Action<Scope, int> visitor = null;
            visitor = (Scope s, int scopeLevel) =>
            {
                if (s.Level.HasValue && s.Level.Value != scopeLevel)
                {
                    throw new BadScopeModelConfigurationException(s.Name);
                }

                s.Level = scopeLevel;
                if (s.ChildIds != null)
                {
                    foreach (var childId in s.ChildIds)
                    {
                        if (scopes[childId].Level.HasValue && scopes[childId].Level.Value <= s.Level)
                        {
                            throw new BadScopeModelConfigurationException(s.Name, scopes[childId].Name, s.Level.Value, scopes[childId].Level.Value);
                        }

                        visitor(scopes[childId], scopeLevel + 1);
                    }
                }
            };

            var rootScopes = scopes.Values.Where(s => s.ParentIds == null || !s.ParentIds.Any()).ToList();
            foreach(var rootScope in rootScopes)
            {
                visitor(rootScope, 0);
            }
        }

        private async Task<TCacheableObject> GetOrCreateAsync<TCacheableObject>(
            string cacheKey,
            Func<Task<TCacheableObject>> queryCacheableObject,
            Func<Data.ModelModificationDate, DateTime> getModificationDate)
            where TCacheableObject : class, ICacheableObject
        {
            TCacheableObject cacheableObject;
            if (this.memoryCache != null)
            {
                cacheableObject = this.memoryCache.Get<TCacheableObject>(cacheKey);
                
                var databaseModelModificationDate = await SharedQueries.GetModelModificationDateAsync(this.context);
                
                var modificationDate = getModificationDate(databaseModelModificationDate);                
                if (cacheableObject != null && modificationDate <= cacheableObject.CacheValuesDateTime)
                {
                    return cacheableObject;
                }
                
                cacheableObject = await queryCacheableObject();
                cacheableObject.CacheValuesDateTime = modificationDate;
                
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetPriority(CacheItemPriority.NeverRemove);

                this.memoryCache.Set(cacheKey, cacheableObject, cacheEntryOptions);
                
                return cacheableObject;
            }

            return await queryCacheableObject();            
        }
    }
}
