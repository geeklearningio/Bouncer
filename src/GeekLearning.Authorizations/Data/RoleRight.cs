namespace GeekLearning.Authorizations.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class RoleRight
    {
        public Guid RoleId { get; set; }
       
        public Guid RightId { get; set; }

        [ForeignKey(nameof(RoleId))]
        [InverseProperty(nameof(Data.Role.Rights))]
        public virtual Role Role { get; set; }

        [ForeignKey(nameof(RightId))]
        [InverseProperty(nameof(Data.Right.Roles))]
        public virtual Right Right { get; set; }
    }
}
