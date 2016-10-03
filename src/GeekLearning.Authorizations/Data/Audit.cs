namespace GeekLearning.Authorizations.Data
{
    using System;

    public abstract class Audit
    {
        public DateTime CreationDate { get; set; }

        public Guid CreationBy { get; set; }

        public DateTime ModificationDate { get; set; }

        public Guid ModificationBy { get; set; }
    }
}
