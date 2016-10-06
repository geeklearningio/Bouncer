namespace GeekLearning.Authorizations.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Right : Audit
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(Constants.ColumnNameLength)]
        public string Name { get; set; }

        public bool IsDeletable { get; set; } = true;

        public virtual ICollection<RoleRight> Roles { get; set; }
    }
}
