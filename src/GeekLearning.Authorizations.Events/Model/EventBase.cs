namespace GeekLearning.Authorizations.Events.Model
{
    using System;

    public abstract class EventBase : IEquatable<EventBase>
    {
        public EventBase()
        {
        }

        public EventBase(string key)
        {
            this.Key = key;
        }

        public string Key { get; set; }

        public bool Equals(EventBase other)
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

            if (this.Key == other.Key)
            {
                return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as EventBase);
        }

        public static bool operator ==(EventBase a, EventBase b)
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

        public static bool operator !=(EventBase a, EventBase b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return this.Key.GetHashCode();
        }
    }
}
