namespace GeekLearning.Authorizations.Data
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Authorization : Audit
    {
        [Key]
        public Guid Id { get; set; }

        public bool IsDeletable { get; set; }
        
        public Guid RoleId { get; set; }

        [Required]
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }

        public Guid ScopeId { get; set; }

        [Required]
        [ForeignKey("ScopeId")]
        public virtual Scope Scope { get; set; }

        public Guid PrincipalId { get; set; }

        [Required]
        [ForeignKey("PrincipalId")]
        public virtual Principal Principal { get; set; }
    }
}
