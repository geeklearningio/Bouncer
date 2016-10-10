namespace GeekLearning.Authorizations.Projections
{
    using GeekLearning.Authorizations.Model;
    using System;
    using System.Data.Common;
    using System.Linq;
    using System.Threading.Tasks;

    public static class RightsResultProjections
    {
        public static async Task<RightsResult> ToRightsResultAsync(this DbDataReader reader)
        {
            RightsResult rightsResult = new RightsResult();

            while (await reader.ReadAsync())
            {
                ScopeRights right = new ScopeRights();

                right.ScopeId = Guid.Parse(reader["ScopeId"].ToString());
                right.ScopeName = reader["ScopeName"] as string;
                var inheritedRights = reader["InheritedRights"] as string;
                if (inheritedRights != null)
                {
                    right.InheritedRightKeys = inheritedRights.Split(',');
                }

                var explicitRights = reader["ExplicitRights"] as string;
                if (explicitRights != null)
                {
                    right.ExplicitRightKeys = explicitRights.Split(',');
                }

                right.ScopeHierarchy = reader["ScopeHierarchy"] as string;

                rightsResult.RightsPerScopeInternal[right.ScopeName] = right;
            }

            return rightsResult;
        }
    }
}
