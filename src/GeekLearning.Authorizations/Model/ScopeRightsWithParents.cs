namespace GeekLearning.Authorizations.Model
{
    using System;
    using System.Collections.Generic;

    public class ScopeRightsWithParents : ScopeRights
    {
        public bool ParentsIterationDone { get; set; }

        public List<Guid> ParentIds { get; set; }
    }
}
