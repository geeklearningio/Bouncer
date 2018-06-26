namespace GeekLearning.Authorizations.EntityFrameworkCore.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Right : Audit
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(Constants.ColumnNameLength)]
        public string Name { get; set; }

        public bool IsDeletable { get; set; } = true;

        [InverseProperty(nameof(RoleRight.Right))]
        public virtual ICollection<RoleRight> Roles { get; set; }
    }
}
