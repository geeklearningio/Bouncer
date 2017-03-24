namespace GeekLearning.Authorizations.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ScopeRights
    {
        private readonly Dictionary<string, Right> rightsOnScope;
        private readonly Dictionary<string, Right> rightsUnderScope;

        public ScopeRights(Guid principalId, string scopeName, IEnumerable<Right> rightsOnScope, IEnumerable<Right> rightsUnderScope)
        {
            this.PrincipalId = principalId;
            this.ScopeName = scopeName;

            this.rightsOnScope = ComputeRights(principalId, scopeName, rightsOnScope);
            this.rightsUnderScope = ComputeRights(principalId, scopeName, rightsOnScope.Union(rightsUnderScope));
        }

        public Guid PrincipalId { get; }

        public string ScopeName { get; }

        public bool HasAnyExplicitRight 
            => this.rightsOnScope.Values.Any(r => r.IsExplicit);

        public bool HasAnyRightUnder 
            => this.rightsUnderScope.Values.Any();

        public bool HasRight(string right) 
            => this.rightsOnScope.ContainsKey(right);

        public bool HasInheritedRight(string right) 
            => this.rightsOnScope.ContainsKey(right) && !this.rightsOnScope[right].IsExplicit;

        public bool HasExplicitRight(string right) 
            => this.rightsOnScope.ContainsKey(right) && this.rightsOnScope[right].IsExplicit;

        public bool HasRightUnder(string right) 
            => this.rightsUnderScope.ContainsKey(right);

        private static Dictionary<string, Right> ComputeRights(Guid principalId, string scopeName, IEnumerable<Right> rights)
        {
            if (rights != null)
            {
                return rights
                    .GroupBy(r => r.RightName)
                    .ToDictionary(rg => rg.Key, rg => new Right(principalId, scopeName, rg.Key, rg.Any(r => r.IsExplicit)));
            }

            return new Dictionary<string, Right>();
        }
    }
}
