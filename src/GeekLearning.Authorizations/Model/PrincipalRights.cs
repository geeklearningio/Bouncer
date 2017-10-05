namespace GeekLearning.Authorizations.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class PrincipalRights
    {
        public PrincipalRights()
        {
        }

        public PrincipalRights(Guid principalId, string rootScopeName, IEnumerable<ScopeRights> ScopeRights)
        {
            this.PrincipalId = principalId;
            this.RootScopeName = rootScopeName;
            this.ScopeRights = ComputeScopes(principalId, ScopeRights);
        }

        public Guid PrincipalId { get; set; }

        public string RootScopeName { get; set; }
        
        public IReadOnlyDictionary<string, ScopeRights> ScopeRights { get; set; }

        public bool HasRightOnScope(string right, string scope) 
            => this.ScopeRights.ContainsKey(scope) && this.ScopeRights[scope].HasRight(right);

        public bool HasInheritedRightOnScope(string right, string scope) 
            => this.ScopeRights.ContainsKey(scope) && this.ScopeRights[scope].HasInheritedRight(right);

        public bool HasExplicitRightOnScope(string right, string scope) 
            => this.ScopeRights.ContainsKey(scope) && this.ScopeRights[scope].HasExplicitRight(right);

        public bool HasAnyExplicitRightOnScope(string scope) 
            => this.ScopeRights.ContainsKey(scope) && this.ScopeRights[scope].HasAnyExplicitRight;

        public bool HasAnyRightUnderScope(string scope)
            => this.ScopeRights.ContainsKey(scope) && this.ScopeRights[scope].HasAnyRightUnder;

        public bool HasRightUnderScope(string right, string scope)
            => this.ScopeRights.ContainsKey(scope) && this.ScopeRights[scope].HasRightUnder(right);

        private static Dictionary<string, ScopeRights> ComputeScopes(Guid principalId, IEnumerable<ScopeRights> ScopeRights)
        {
            if (ScopeRights != null)
            {
                return ScopeRights.ToDictionary(s => s.ScopeName, s => s);
            }

            return new Dictionary<string, ScopeRights>();
        }
    }
}
