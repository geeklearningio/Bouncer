namespace GeekLearning.Authorizations.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class RoleRight
    {
        public Guid RoleId { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
       
        public Guid RightId { get; set; }

        [ForeignKey("RightId")]
        public virtual Right Right { get; set; }
    }
}
