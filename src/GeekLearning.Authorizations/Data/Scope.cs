﻿namespace GeekLearning.Authorizations.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Threading.Tasks;

    public class Scope : Audit
    {
        public Scope()
        {
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

        [InverseProperty(nameof(ScopeHierarchy.Parent))]
        public virtual ICollection<ScopeHierarchy> Parents { get; set; }

        [InverseProperty(nameof(ScopeHierarchy.Child))]
        public virtual ICollection<ScopeHierarchy> Children { get; set; }
    }
}
