namespace GeekLearning.Authorizations.Model
{
    using System.Collections.Generic;
    using System.Linq;

    public class RightsResult
    {
        public IDictionary<string, IList<Right>> RightsPerContext => new Dictionary<string, IList<Right>>();

        public bool HasRightOnScope(string right, string scope)
        {
            IList<Right> rightsForScope;
            if (RightsPerContext.TryGetValue(scope, out rightsForScope))
            {
                return rightsForScope.Any(r => r.Key == right);                
            }

            return false;
        }
    }
}
