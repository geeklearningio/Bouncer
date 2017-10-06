namespace GeekLearning.Authorizations.Events.Model
{
    using System;

    public class AffectRoleToPrincipalOnScope : EventBase
    {
        public Guid PrincipalId { get; set; }

        public string RoleName { get; set; }

        public string ScopeName { get; set; }

        public AffectRoleToPrincipalOnScope()
        {
        }

        public AffectRoleToPrincipalOnScope(Guid principalId, string roleName, string scopeName) : base($"{principalId.ToString()}_{roleName}_{scopeName}")
        {
        }
    }
}
