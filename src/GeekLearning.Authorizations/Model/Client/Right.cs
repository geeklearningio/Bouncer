namespace GeekLearning.Authorizations.Model.Client
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public class Right : IEquatable<Right>
    {
        public Right(Guid principalId, string scopeName, string rightName, bool isExplicit, Guid sourceAuthorization)
            : this(principalId, scopeName, rightName, isExplicit, new Guid[] { sourceAuthorization })
        {
        }

        [JsonConstructor]
        public Right(Guid principalId, string scopeName, string rightName, bool isExplicit, IEnumerable<Guid> sourceAuthorizations)
        {
            this.PrincipalId = principalId;
            this.SourceAuthorizations = sourceAuthorizations;
            this.ScopeName = scopeName;
            this.RightName = rightName;
            this.IsExplicit = isExplicit;
        }

        public Guid PrincipalId { get; }

        public IEnumerable<Guid> SourceAuthorizations { get; }

        public string ScopeName { get; }

        public string RightName { get; }

        public bool IsExplicit { get; }

        public bool Equals(Right other)
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

            if (this.PrincipalId == other.PrincipalId
                && this.ScopeName == other.ScopeName
                && this.RightName == other.RightName
                && this.IsExplicit == other.IsExplicit)
            {
                return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Right);
        }

        public static bool operator ==(Right a, Right b)
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

        public static bool operator !=(Right a, Right b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return string.Join(
                "-",
                this.PrincipalId.ToString(),
                this.ScopeName,
                this.RightName,
                this.IsExplicit.ToString()).GetHashCode();
        }
    }
}
