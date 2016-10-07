namespace GeekLearning.Authorizations.Model
{
    using System;
    using System.Collections.Generic;

    public class ScopeRights
    {
        public Guid ScopeId { get; set; }

        public string ScopeName { get; set; }

        public IEnumerable<string> InheritedRightKeys { get; set; }

        public IEnumerable<string> ExplicitRightKeys { get; set; }

        public string ScopeHierarchy { get; set; }
    }
}
