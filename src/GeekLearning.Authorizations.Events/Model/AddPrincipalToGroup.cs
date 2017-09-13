namespace GeekLearning.Authorizations.Events.Model
{
    using System;

    public class AddPrincipalToGroup : EventBase
    {
        public Guid PrincipalId { get; set; }

        public string GroupName { get; set; }

        public AddPrincipalToGroup()
        {                
        }

        public AddPrincipalToGroup(Guid principalId, string groupName) : base($"{principalId.ToString()}_{groupName}")
        {
        }
    }
}
