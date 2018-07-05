namespace GeekLearning.Bouncer.EntityFrameworkCore.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Membership : Audit
    {
        public Guid PrincipalId { get; set; }

        [Required]
        [ForeignKey(nameof(PrincipalId))]
        public Principal Principal { get; set; }

        public Guid GroupId { get; set; }

        [Required]
        [ForeignKey(nameof(GroupId))]
        public Group Group { get; set; }

        public bool IsDeletable { get; set; } = true;
    }
}
