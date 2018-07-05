namespace GeekLearning.Bouncer.AspNetCore
{
    using Microsoft.AspNetCore.Authorization;

    public abstract class AclAuthorizationRequirement : IAuthorizationRequirement
    {
        public AclAuthorizationRequirement(string rightName, string scopeName)
        {
            this.RightName = rightName;
            this.ScopeName = scopeName; 
        }

        public string RightName { get; set; }

        public string ScopeName { get; set; }
    }
}
