namespace GeekLearning.Authorizations.Events.Model
{
    using System;
    using System.Collections.Generic;

    public class AuthorizationsImpact
    {
        public IList<string> ScopeNames { get; set; } = new List<string>();

        public IList<Guid> UserIds { get; set; } = new List<Guid>();
    }
}
