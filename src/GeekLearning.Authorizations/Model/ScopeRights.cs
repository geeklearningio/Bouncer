namespace GeekLearning.Authorizations.Model
{
    using Newtonsoft.Json;
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

        [JsonIgnore]
        public IEnumerable<string> InheritedRightKeys { get; set; }

        public IEnumerable<string> ExplicitRightKeys { get; set; }

        [JsonIgnore]
        public IEnumerable<string> ScopeHierarchies { get; set; }
    }
}
