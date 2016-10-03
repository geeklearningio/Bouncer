namespace GeekLearning.Authorizations.Data
{
    using System;

    public class Principal : Audit
    {
        public Guid Id { get; set; }

        public bool IsDeletable { get; set; }
    }
}
