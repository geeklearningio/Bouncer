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

        public bool IsDeletable { get; set; } = true;
        
        public Guid RoleId { get; set; }

        [Required]
        [ForeignKey(nameof(RoleId))]
        public virtual Role Role { get; set; }

        public Guid ScopeId { get; set; }

        [Required]
        [ForeignKey(nameof(ScopeId))]
        [InverseProperty(nameof(Data.Scope.Authorizations))]
        public virtual Scope Scope { get; set; }

        public Guid PrincipalId { get; set; }

        [Required]
        [ForeignKey(nameof(PrincipalId))]
        public virtual Principal Principal { get; set; }
    }
}
