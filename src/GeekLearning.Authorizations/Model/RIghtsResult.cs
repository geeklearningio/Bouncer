namespace GeekLearning.Authorizations.Model
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class RightsResult
    {
        public RightsResult()
        {
        }

        public RightsResult(IEnumerable<ScopeRights> scopeRights)
        {
            this.RightsPerScopeInternal = scopeRights.ToDictionary(sr => sr.ScopeName);
        }

        internal IDictionary<string, ScopeRights> RightsPerScopeInternal { get; set; } = new Dictionary<string, ScopeRights>();

        private IReadOnlyDictionary<string, ScopeRights> rightsPerScope;
        public IReadOnlyDictionary<string, ScopeRights> RightsPerScope
        {
            get
            {
                return this.rightsPerScope ?? (this.rightsPerScope = new ReadOnlyDictionary<string, ScopeRights>(this.RightsPerScopeInternal));
            }
        }

        private IEnumerable<string> scopesWithRights;
        public IEnumerable<string> ScopesWithRights
        {
            get
            {
                return this.scopesWithRights ?? (this.scopesWithRights = ComputeScopedRights());
            }
        }

        private IEnumerable<string> ComputeScopedRights()
        {
            return this.RightsPerScope
                       .Values
                       .SelectMany(sr => sr.ScopeHierarchies.SelectMany(sh => sh.Split('/')))
                       .Distinct()
                       .ToList();
        }

        public bool HasRightOnScope(string right, string scope)
        {
            ScopeRights rightsForScope;
            if (RightsPerScope.TryGetValue(scope, out rightsForScope))
            {
                return rightsForScope.InheritedRightKeys.Contains(right);
            }

            return false;
        }

        public bool HasAnyRightUnderScope(string scope)
        {
            return this.ScopesWithRights.Contains(scope);
        }

        public bool HasRightUnderScope(string scope, string right)
        {
            return this.RightsPerScope
                       .Values
                       .Where(rs => rs.InheritedRightKeys.Contains(right) && rs.ScopeHierarchies.Any(sh => sh.Contains(scope)))
                       .Any();
        }
    }
}
