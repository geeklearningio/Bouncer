namespace GeekLearning.Authorizations.Data
{
    using System;

    public abstract class Audit
    {
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        public Guid CreationBy { get; set; }

        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        public Guid ModificationBy { get; set; }
    }
}
