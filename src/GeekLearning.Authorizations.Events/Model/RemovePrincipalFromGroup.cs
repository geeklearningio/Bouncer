namespace GeekLearning.Authorizations.Events.Model
{
    using System;

    public class RemovePrincipalFromGroup : EventBase
    {
        public Guid PrincipalId { get; set; }

        public string GroupName { get; set; }

        public RemovePrincipalFromGroup()
        {
        }

        public RemovePrincipalFromGroup(Guid principalId, string groupName) : base($"{principalId.ToString()}_{groupName}")
        {
        }
    }
}
