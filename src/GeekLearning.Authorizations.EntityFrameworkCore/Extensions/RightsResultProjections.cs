﻿namespace GeekLearning.Authorizations.EntityFrameworkCore
{
    using Model;
    using Microsoft.EntityFrameworkCore.Storage;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static class RightsResultProjections
    {
        public static async Task<RightsResult> FromInheritedResultToRightsResultAsync(this RelationalDataReader relationalReader)
        {
            var scopeRights = new List<ScopeRights>();
            var reader = relationalReader.DbDataReader;

            while (await reader.ReadAsync())
            {
                var right = new ScopeRights();

                right.ScopeId = Guid.Parse(reader["ScopeId"].ToString());
                right.ScopeName = reader["ScopeName"] as string;
                var inheritedRights = reader["InheritedRights"] as string;
                if (inheritedRights != null)
                {
                    right.RightKeys = inheritedRights.Split(',');
                }

                var explicitRights = reader["ExplicitRights"] as string;
                if (explicitRights != null)
                {
                    right.ExplicitRightKeys = explicitRights.Split(',');
                }

                right.ScopeHierarchies = new List<string> { reader["ScopeHierarchy"] as string };

                scopeRights.Add(right);
            }

            return new RightsResult(scopeRights);
        }

        public async static Task<IEnumerable<ScopeRightsWithParents>> FromFlatResultToRightsResultAsync(this RelationalDataReader relationalReader)
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

            return principalScopeRights
                .GroupBy(psr => psr.ScopeId)
                .Select(g => new ScopeRightsWithParents
                {
                    ScopeId = g.First().ScopeId,
                    ParentIds = g.Where(psr => psr.ParentScopeId.HasValue).Select(psr => psr.ParentScopeId.Value).ToList(),
                    ScopeName = g.First().ScopeName,
                    ExplicitRightKeys = g.Where(psr => !string.IsNullOrEmpty(psr.RightName)).Select(psr => psr.RightName).ToList(),
                    RightKeys = new List<string>(),
                    ScopeHierarchies = new List<string>(),
                })
                .ToList();
        }

        public static IEnumerable<ScopeRights> ParseInheritedRights(this IEnumerable<ScopeRightsWithParents> rights)
        {
            var indexedRights = rights.ToDictionary(r => r.ScopeId, r => r);

            foreach (var right in indexedRights)
            {
                if (!right.Value.ParentsIterationDone)
                {
                    IterateThroughParents(right.Value, indexedRights);
                }
            }

            return indexedRights
                .Where(r => r.Value.RightKeys.Any())
                .Select(r => r.Value)
                .ToList();
        }

        public static RightsResult GetResultForScopeName(this IEnumerable<ScopeRights> originalRights, string scopeKey, bool withChildren)
        {
            if (originalRights == null)
            {
                return new RightsResult();
            }

            if (withChildren)
            {
                return new RightsResult(originalRights
                    .Where(r => r.ScopeHierarchies.Any(sh => sh.Contains(scopeKey)))
                    .ToList());
            }

            var right = originalRights.FirstOrDefault(r => r.ScopeName == scopeKey);
            if (right != null)
            {
                return new RightsResult(new List<ScopeRights> { right });
            }

            return new RightsResult();
        }

        private static void IterateThroughParents(ScopeRightsWithParents right, IReadOnlyDictionary<Guid, ScopeRightsWithParents> rights)
        {
            if (right.ParentsIterationDone)
            {
                return;
            }

            var parents = new List<ScopeRightsWithParents>();
            foreach (var parentId in right.ParentIds)
            {
                var parent = rights[parentId];
                if (!parent.ParentsIterationDone)
                {
                    IterateThroughParents(parent, rights);
                }

                parents.Add(parent);
            }

            right.InheritedRightKeys = parents.SelectMany(p => p.RightKeys)
                .Distinct()
                .ToList();

            right.RightKeys = right.ExplicitRightKeys
               .Concat(right.InheritedRightKeys)
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
