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

        public Guid Id { get; set; }

        [Required]
        [StringLength(Constants.ColumnNameLength)]
        public string Name { get; set; }

        public bool IsDeletable { get; set; }

        public ICollection<RoleRight> Rights { get; set; }
    }
}
