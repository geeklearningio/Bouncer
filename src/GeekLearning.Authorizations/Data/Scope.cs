namespace GeekLearning.Authorizations.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;

    public class Scope : Audit
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(Constants.ColumnNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(Constants.ColumnDescriptionLength)]
        public string Description { get; set; }

        public bool IsDeletable { get; set; }

        public virtual ICollection<ScopeHierarchy> Parents { get; set; }

        public virtual ICollection<ScopeHierarchy> Children { get; set; }
    }
}
