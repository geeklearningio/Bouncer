namespace GeekLearning.Authorizations.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Authorization : Audit
    {
        public Guid Id { get; set; }

        public bool IsDeletable { get; set; }

        public Guid RoleId { get; set; }

        [Required]
        public Role Role { get; set; }

        public Guid ScopeId { get; set; }

        [Required]
        public Scope Scope { get; set; }

        public Guid PrincipalId { get; set; }

        [Required]
        public Principal Principal { get; set; }
    }
}
