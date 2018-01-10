namespace GeekLearning.Authorizations.Model.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ScopeRights
    {
        public ScopeRights(Guid principalId, string scopeName, IEnumerable<Right> rightsOnScope, IEnumerable<Right> rightsUnderScope)
        {
            this.PrincipalId = principalId;
            this.ScopeName = scopeName;

            this.RightsOnScope = ComputeRights(principalId, scopeName, rightsOnScope);

            var rightsOnAndUnderScopes = new List<Right>(rightsOnScope);
            rightsOnAndUnderScopes.AddRange(rightsUnderScope);

            this.RightsUnderScope = ComputeRights(principalId, scopeName, rightsOnAndUnderScopes);
        }

        public Guid PrincipalId { get; set; }

        public string ScopeName { get; set; }

        public IReadOnlyDictionary<string, Right> RightsOnScope { get; set; }

        public IReadOnlyDictionary<string, Right> RightsUnderScope { get; set; }

        public bool HasAnyExplicitRight
            => this.RightsOnScope.Values.Any(r => r.IsExplicit);

        public bool HasAnyRightUnder
            => this.RightsUnderScope.Values.Any();

        public bool HasRight(string right)
            => this.RightsOnScope.ContainsKey(right);

        public bool HasInheritedRight(string right)
            => this.RightsOnScope.ContainsKey(right) && !this.RightsOnScope[right].IsExplicit;

        public bool HasExplicitRight(string right)
            => this.RightsOnScope.ContainsKey(right) && this.RightsOnScope[right].IsExplicit;

        public bool HasRightUnder(string right)
            => this.RightsUnderScope.ContainsKey(right);

        private static Dictionary<string, Right> ComputeRights(Guid principalId, string scopeName, IEnumerable<Right> rights)
        {
            if (rights?.Any() == true)
            {
                return rights
                    .GroupBy(r => r.RightName)
                    .ToDictionary(rg => rg.Key, rg => new Right(principalId, scopeName, rg.Key, rg.Any(r => r.IsExplicit)));
            }

            return new Dictionary<string, Right>();
        }
    }
}
