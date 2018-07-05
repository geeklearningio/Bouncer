namespace GeekLearning.Bouncer.EntityFrameworkCore.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

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

        [InverseProperty(nameof(RoleRight.Role))]
        public virtual ICollection<RoleRight> Rights { get; set; }
    }
}
