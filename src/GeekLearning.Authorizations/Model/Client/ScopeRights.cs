namespace GeekLearning.Authorizations.Model.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ScopeRights : IEquatable<ScopeRights>
    {
        public ScopeRights(Guid principalId, string scopeName, IEnumerable<Right> rightsOnScope, IEnumerable<Right> rightsUnderScope)
        {
            this.PrincipalId = principalId;
            this.ScopeName = scopeName;

            this.RightsOnScope = ComputeRights(principalId, scopeName, rightsOnScope);
            this.RightsUnderScope = ComputeRights(principalId, scopeName, rightsOnScope.Union(rightsUnderScope));
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

        public bool Equals(ScopeRights other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (this.GetType() != other.GetType())
            {
                return false;
            }

            if (this.PrincipalId == other.PrincipalId && this.ScopeName == this.ScopeName)
            {
                return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as ScopeRights);
        }

        public static bool operator ==(ScopeRights a, ScopeRights b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
            {
                return true;
            }

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(ScopeRights a, ScopeRights b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return string.Join(
                "-",
                this.PrincipalId.ToString(),
                this.ScopeName).GetHashCode();
        }

        private static Dictionary<string, Right> ComputeRights(Guid principalId, string scopeName, IEnumerable<Right> rights)
        {
            if (rights?.Any() == true)
            {
                return rights
                    .GroupBy(r => r.RightName)
                    .ToDictionary(rg => rg.Key, rg => new Right(principalId, scopeName, rg.Key, rg.Any(r => r.IsExplicit), rg.SelectMany(x => x.SourceAuthorizations)));

            }

            return new Dictionary<string, Right>();
        }
    }
}
