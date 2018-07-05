namespace GeekLearning.Bouncer.EntityFrameworkCore.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ScopeHierarchy
    {
        public Guid ParentId { get; set; }

        public Guid ChildId { get; set; }

        [Required]
        [ForeignKey(nameof(ParentId))]
        [InverseProperty(nameof(Scope.Children))]
        public virtual Scope Parent { get; set; }

        [Required]
        [ForeignKey(nameof(ChildId))]
        [InverseProperty(nameof(Scope.Parents))]
        public virtual Scope Child { get; set; }
    }
}
