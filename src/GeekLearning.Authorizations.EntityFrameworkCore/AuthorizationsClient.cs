namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Model;
    using Data;
    using Newtonsoft.Json;

    public class AuthorizationsClient<TContext> : IAuthorizationsClient where TContext : DbContext
    {
        private readonly TContext context;

        private readonly Guid currentPrincipalId;

        public AuthorizationsClient(TContext context, Guid currentPrincipalId)
        {
            this.context = context;
            this.currentPrincipalId = currentPrincipalId;
        }

        public async Task<RightsResult> GetRightsAsync(string scopeKey, bool withChildren = false)
        {
            RightsResult rightsResult = new RightsResult();
         
            return rightsResult;
        }

        public Task<bool> HasRightAsync(string rightKey, string scopeKey)
        {
            throw new NotImplementedException();
        }
    }
}
