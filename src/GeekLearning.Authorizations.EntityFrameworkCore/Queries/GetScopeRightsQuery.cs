namespace GeekLearning.Authorizations.EntityFrameworkCore.Queries
{
    using GeekLearning.Authorizations.Model.Client;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class GetScopeRightsQuery
    {
        private readonly IDictionary<Guid, Caching.Scope> scopesById;

        private readonly IDictionary<Guid, string[]> explicitRightsByScope;

        private readonly Dictionary<Guid, IEnumerable<Right>> rightsByScope = new Dictionary<Guid, IEnumerable<Right>>();

        public GetScopeRightsQuery(IDictionary<Guid, Caching.Scope> scopesById, IDictionary<Guid, string[]> explicitRightsByScope)
        {
            this.scopesById = scopesById;
            this.explicitRightsByScope = explicitRightsByScope;
        }

        public IEnumerable<ScopeRights> Execute(Guid scopeId, Guid principalId, bool withChildren = false)
        {
            if (!this.scopesById.TryGetValue(scopeId, out Caching.Scope scope))
            {
                // TODO: Log Warning!
                return Enumerable.Empty<ScopeRights>();
            }

            List<ScopeRights> childrenScopesRights = new List<ScopeRights>();
            if (withChildren && scope.ChildIds?.Any() == true)
            {
                foreach (var childScopeId in scope.ChildIds)
                {
                    childrenScopesRights.AddRange(this.Execute(childScopeId, principalId, withChildren));
                }
            }

            var rightsOnScope = this.DetectScopeRights(scopeId, principalId, this.scopesById, this.explicitRightsByScope);
            var rightsUnderScope = childrenScopesRights.SelectMany(sr => sr.RightsOnScope.Values).ToList();
            
            return new List<ScopeRights>(childrenScopesRights)
            {
                new ScopeRights(principalId, scope.Name, rightsOnScope, rightsUnderScope)
            };
        }

        private IEnumerable<Right> DetectScopeRights(
            Guid scopeId,
            Guid principalId,
            IDictionary<Guid, Caching.Scope> scopesById,
            IDictionary<Guid, string[]> explicitRightsByScope)
        {
            if (!scopesById.TryGetValue(scopeId, out Caching.Scope scope))
            {
                // TODO: Log Warning!
                return Enumerable.Empty<Right>();
            }

            if (this.rightsByScope.ContainsKey(scopeId))
            {
                return this.rightsByScope[scopeId];
            }

            List<Right> currentRights = new List<Right>();
            foreach (var parentScopeId in scope.ParentIds ?? Enumerable.Empty<Guid>())
            {
                currentRights.AddRange(this.DetectScopeRights(parentScopeId, principalId, scopesById, explicitRightsByScope));
            }

            if (explicitRightsByScope.TryGetValue(scope.Id, out string[] explicitRightNames))
            {
                currentRights.AddRange(explicitRightNames.Select(rn => new Right(principalId, scope.Name, rn, true)));
            }

            this.rightsByScope[scopeId] = currentRights.Distinct();

            return this.rightsByScope[scopeId];
        }
    }
}
