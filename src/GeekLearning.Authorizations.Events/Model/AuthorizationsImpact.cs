namespace GeekLearning.Authorizations.Events.Model
{
    using System;
    using System.Collections.Generic;

    public class AuthorizationsImpact
    {
        public IList<Guid> ScopeIds { get; set; } = new List<Guid>();

        public IList<Guid> UserIds { get; set; } = new List<Guid>();
    }
}
