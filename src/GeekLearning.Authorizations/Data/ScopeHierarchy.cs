using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GeekLearning.Authorizations.Data
{
    public class ScopeHierarchy
    {
        public Guid ParentId { get; set; }

        public Guid ChildId { get; set; }

        [Required]
        [ForeignKey(nameof(ParentId))]
        [InverseProperty(nameof(Scope.Parents))]
        public virtual Scope Parent { get; set; }

        [Required]
        [ForeignKey(nameof(ChildId))]
        [InverseProperty(nameof(Scope.Children))]
        public virtual Scope Child { get; set; }
    }
}
