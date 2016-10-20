namespace GeekLearning.Authorizations.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ScopeRights
    {
        public ScopeRights()
        {
            this.InheritedRightKeys = Enumerable.Empty<string>();
            this.ExplicitRightKeys = Enumerable.Empty<string>();
        }

        public Guid ScopeId { get; set; }

        public string ScopeName { get; set; }

        public IEnumerable<string> InheritedRightKeys { get; set; }

        public IEnumerable<string> ExplicitRightKeys { get; set; }

        public string ScopeHierarchy { get; set; }
    }
}
