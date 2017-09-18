namespace GeekLearning.Authorizations.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class PrincipalRights
    {
        private readonly Dictionary<string, ScopeRights> scopeRights;

        public PrincipalRights(Guid principalId, string rootScopeName, IEnumerable<ScopeRights> scopeRights, bool scopeNotFound = false)
        {
            this.PrincipalId = principalId;
            this.RootScopeName = rootScopeName;
            this.ScopeNotFound = scopeNotFound;
            this.scopeRights = ComputeScopes(principalId, scopeRights);
        }

        public Guid PrincipalId { get; }

        public string RootScopeName { get; set; }

        public bool ScopeNotFound { get;  }

        public IReadOnlyDictionary<string, ScopeRights> ScopeRights => this.scopeRights;

        public bool HasRightOnScope(string right, string scope) 
            => this.scopeRights.ContainsKey(scope) && this.scopeRights[scope].HasRight(right);

        public bool HasInheritedRightOnScope(string right, string scope) 
            => this.scopeRights.ContainsKey(scope) && this.scopeRights[scope].HasInheritedRight(right);

        public bool HasExplicitRightOnScope(string right, string scope) 
            => this.scopeRights.ContainsKey(scope) && this.scopeRights[scope].HasExplicitRight(right);

        public bool HasAnyExplicitRightOnScope(string scope) 
            => this.scopeRights.ContainsKey(scope) && this.scopeRights[scope].HasAnyExplicitRight;

        public bool HasAnyRightUnderScope(string scope)
            => this.scopeRights.ContainsKey(scope) && this.scopeRights[scope].HasAnyRightUnder;

        public bool HasRightUnderScope(string right, string scope)
            => this.scopeRights.ContainsKey(scope) && this.scopeRights[scope].HasRightUnder(right);

        private static Dictionary<string, ScopeRights> ComputeScopes(Guid principalId, IEnumerable<ScopeRights> scopeRights)
        {
            if (scopeRights != null)
            {
                return scopeRights.ToDictionary(s => s.ScopeName, s => s);
            }

            return new Dictionary<string, ScopeRights>();
        }
    }
}
