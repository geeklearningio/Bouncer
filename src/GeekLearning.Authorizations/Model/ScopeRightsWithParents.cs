namespace GeekLearning.Authorizations.Model
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public class ScopeRightsWithParents : ScopeRights
    {
        [JsonIgnore]
        public bool ParentsIterationDone { get; set; }

        public List<Guid> ParentIds { get; set; }
    }
}
