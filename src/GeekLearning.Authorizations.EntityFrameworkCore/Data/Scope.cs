namespace GeekLearning.Authorizations.EntityFrameworkCore.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Scope : Audit
    {
        public Scope()
        {
            Authorizations = new HashSet<Authorization>();
            Parents = new HashSet<ScopeHierarchy>();
            Children = new HashSet<ScopeHierarchy>();
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(Constants.ColumnNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(Constants.ColumnDescriptionLength)]
        public string Description { get; set; }

        public bool IsDeletable { get; set; } = true;

        [InverseProperty(nameof(Authorization.Scope))]
        public virtual ICollection<Authorization> Authorizations { get; set; }

        [InverseProperty(nameof(ScopeHierarchy.Parent))]
        public virtual ICollection<ScopeHierarchy> Children { get; set; }

        [InverseProperty(nameof(ScopeHierarchy.Child))]
        public virtual ICollection<ScopeHierarchy> Parents { get; set; }
    }
}
