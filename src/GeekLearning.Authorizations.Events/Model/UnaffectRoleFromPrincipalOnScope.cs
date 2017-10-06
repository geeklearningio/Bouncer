namespace GeekLearning.Authorizations.Events.Model
{
    using System;

    public class UnaffectRoleFromPrincipalOnScope : EventBase
    {
        public Guid PrincipalId { get; set; }

        public string RoleName { get; set; }

        public string ScopeName { get; set; }

        public UnaffectRoleFromPrincipalOnScope()
        {
        }

        public UnaffectRoleFromPrincipalOnScope(Guid principalId, string roleName, string scopeName) : base($"{principalId.ToString()}_{roleName}_{scopeName}")
        {
        }
    }
}
