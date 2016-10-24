﻿namespace GeekLearning.Authorizations.Projections
{
    using GeekLearning.Authorizations.Model;
    using Microsoft.EntityFrameworkCore.Storage;
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using System.Threading.Tasks;

    public static class RightsResultProjections
    {
        public static async Task<RightsResult> FromInheritedResultToRightsResultAsync(this RelationalDataReader relationalReader)
        {
            RightsResult rightsResult = new RightsResult();

            var reader = relationalReader.DbDataReader;

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

                right.ScopeHierarchies = new List<string> { reader["ScopeHierarchy"] as string };

                rightsResult.RightsPerScopeInternal[right.ScopeName] = right;
            }

            return rightsResult;
        }

        public async static Task<List<ScopeRightsWithParents>> FromFlatResultToRightsResultAsync(this RelationalDataReader relationalReader)
        {
            var principalScopeRights = new List<PrincipalScopeRight>();
            var reader = relationalReader.DbDataReader;

            while (await reader.ReadAsync())
            {
                var right = new PrincipalScopeRight();
                right.PrincipalId = (Guid)reader["PrincipalId"];
                right.ScopeId = (Guid)reader["ScopeId"];
                right.ParentScopeId = reader["ParentScopeId"] is DBNull ? (Guid?)null : (Guid)reader["ParentScopeId"];
                right.ScopeName = reader["ScopeName"] as string;
                right.RightName = reader["RightName"] as string;
                principalScopeRights.Add(right);
            }

            var rights = principalScopeRights
                .GroupBy(psr => psr.ScopeId)
                .Select(g => new ScopeRightsWithParents
                {
                    ScopeId = g.First().ScopeId,
                    ParentIds = g.Where(psr => psr.ParentScopeId.HasValue).Select(psr => psr.ParentScopeId.Value).ToList(),
                    ScopeName = g.First().ScopeName,
                    ExplicitRightKeys = g.Where(psr => !string.IsNullOrEmpty(psr.RightName)).Select(psr => psr.RightName).ToList(),
                    InheritedRightKeys = new List<string>(),
                    ScopeHierarchies = new List<string>(),
                })
                .ToList();

            return rights
                .Select(r => IterateThroughParents(r, rights))
                .Where(r => r.InheritedRightKeys.Any())
                .ToList();
        }

        public static RightsResult GetResultForScopeName(this IEnumerable<ScopeRightsWithParents> rights, string scopeKey)
        {
            var rightsResult = new RightsResult();
            foreach (var right in rights.Where(r => r.ScopeHierarchies.Any(sh => sh.Contains(scopeKey))))
            {
                rightsResult.RightsPerScopeInternal[right.ScopeName] = right;
            }

            return rightsResult;
        }

        private static ScopeRightsWithParents IterateThroughParents(ScopeRightsWithParents right, List<ScopeRightsWithParents> rights)
        {
            if (right.ParentsIterationDone)
            {
                return right;
            }

            var parents = rights
                .Where(s => right.ParentIds.Contains(s.ScopeId))
                .Select(s => IterateThroughParents(s, rights))
                .ToList();

            right.InheritedRightKeys = right.ExplicitRightKeys
                .Concat(parents.SelectMany(p => p.InheritedRightKeys))
                .Distinct()
                .ToList();

            if (parents.Any())
            {
                right.ScopeHierarchies = parents
                    .SelectMany(p => p.ScopeHierarchies.Select(sh => $"{sh}/{right.ScopeName}"))
                    .ToList();
            }
            else
            {
                right.ScopeHierarchies = new List<string> { right.ScopeName };
            }

            right.ParentsIterationDone = true;
            return right;
        }

        private class PrincipalScopeRight
        {
            public Guid PrincipalId { get; set; }

            public Guid ScopeId { get; set; }

            public Guid? ParentScopeId { get; set; }

            public string ScopeName { get; set; }

            public string RightName { get; set; }
        }
    }
}
