namespace GeekLearning.Authorizations.Model
{
    using System.Collections.Generic;
    using System.Linq;

    public class RightsResult
    {
        public IDictionary<string, ScopeRights> RightsPerScope { get; set; } = new Dictionary<string, ScopeRights>();

        public bool HasRightOnScope(string right, string scope)
        {
            ScopeRights rightsForScope;
            if (RightsPerScope.TryGetValue(scope, out rightsForScope))
            {
                return rightsForScope.InheritedRightKeys.Contains(right);
            }

            return false;
        }
    }
}
