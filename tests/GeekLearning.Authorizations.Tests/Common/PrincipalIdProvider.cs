namespace GeekLearning.Authorizations.Tests
{
    using System;

    public class PrincipalIdProvider : IPrincipalIdProvider
    {
        private AuthorizationsTestContext context;

        public PrincipalIdProvider(AuthorizationsTestContext context)
        {
            this.context = context;
        }

        public Guid PrincipalId
        {
            get
            {
                return this.context.CurrentUserId;
            }
        }
    }
}
