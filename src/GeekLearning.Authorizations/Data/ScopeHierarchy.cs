using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GeekLearning.Authorizations.Data
{
    public class ScopeHierarchy
    {
        public Guid ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual Scope Parent { get; set; }

        public Guid ChildId { get; set; }

        [ForeignKey("ChildId")]
        public virtual Scope Child { get; set; }
    }
}
