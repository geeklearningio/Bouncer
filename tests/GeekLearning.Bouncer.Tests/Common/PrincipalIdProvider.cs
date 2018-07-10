namespace GeekLearning.Bouncer.Tests
{
    using System;

    public class PrincipalIdProvider : IPrincipalIdProvider
    {
        public PrincipalIdProvider(Guid currentUserId)
        {
            this.PrincipalId = currentUserId;
        }

        public Guid PrincipalId { get; }
    }
}
