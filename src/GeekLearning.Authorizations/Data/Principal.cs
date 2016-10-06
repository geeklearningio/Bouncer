namespace GeekLearning.Authorizations.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Principal : Audit
    {
        [Key]
        public Guid Id { get; set; }

        public bool IsDeletable { get; set; } = true;
    }
}
