namespace GeekLearning.Authorizations.Model
{
    using System;

    public class Right : IEquatable<Right>
    {
        public Right(Guid principalId, string scopeName, string rightName, bool isExplicit)
        {
            this.PrincipalId = principalId;
            this.ScopeName = scopeName;
            this.RightName = rightName;
            this.IsExplicit = isExplicit;
        }

        public Guid PrincipalId { get; }

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
                && this.ScopeName == this.ScopeName
                && this.RightName == this.RightName
                && this.IsExplicit == this.IsExplicit)
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
