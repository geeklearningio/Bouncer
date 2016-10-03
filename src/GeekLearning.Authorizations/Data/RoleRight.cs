namespace GeekLearning.Authorizations.Data
{
    using System;

    public class RoleRight
    {
        public Guid RoleId { get; set; }

        public Role Role { get; set; }

        public Right RightId { get; set; }

        public Right Right { get; set; }
    }
}
