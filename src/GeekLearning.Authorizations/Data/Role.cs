namespace GeekLearning.Authorizations.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Role : Audit
    {
        public Role()
        {
            this.Rights = new HashSet<RoleRight>();
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(Constants.ColumnNameLength)]
        public string Name { get; set; }

        public bool IsDeletable { get; set; } = true;

        public virtual ICollection<RoleRight> Rights { get; set; }
    }
}
