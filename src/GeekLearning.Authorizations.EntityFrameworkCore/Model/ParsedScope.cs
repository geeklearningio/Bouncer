namespace GeekLearning.Authorizations.EntityFrameworkCore.Model
{
    using Authorizations.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class ParsedScope
    {
        public Caching.Scope Scope { get; private set; }

        public IList<string> ExplicitRightNames { get; private set; } = new List<string>();

        public IList<string> RightNames { get; private set; } = new List<string>();

        public IList<string> RightUnderNames { get; private set; } = new List<string>();

        public IList<ParsedScope> Children { get; set; } = new List<ParsedScope>();

        public PrincipalRights ToPrincipalRights(Guid principalId)
        {
            var scopeRights = new Dictionary<Guid, ScopeRights>();

            this.AddToScopeRightsList(principalId, scopeRights);

            return new PrincipalRights(principalId, this.Scope.Name, scopeRights.Values);
        }

        private void AddToScopeRightsList(Guid principalId, Dictionary<Guid, ScopeRights> scopeRights)
        {
            if (!scopeRights.ContainsKey(this.Scope.Id))
            {
                scopeRights.Add(this.Scope.Id, this.ToScopeRights(principalId));
            }

            foreach (var child in this.Children)
            {
                child.AddToScopeRightsList(principalId, scopeRights);
            }
        }

        private ScopeRights ToScopeRights(Guid principalId)
        {
            var rightsOnScope = this.RightNames
                .Select(r => new Right(principalId, this.Scope.Name, r, this.ExplicitRightNames.Contains(r)))
                .ToList();

            var rightsUnderScope = this.RightUnderNames
                .Select(r => new Right(principalId, this.Scope.Name, r, false))
                .ToList();

            return new ScopeRights(principalId, this.Scope.Name, rightsOnScope, rightsUnderScope);
        }

        public static void Parse(
            Guid scopeId,
            IDictionary<Guid, Caching.Scope> scopes, 
            Dictionary<Guid, string[]> principalRightsPerScope, 
            Dictionary<Guid, ParsedScope> parsedScopes)
        {
            Caching.Scope scope;
            if (!scopes.TryGetValue(scopeId, out scope))
            {
                // TODO: Log Warning!
                return;
            }

            ParsedScope parsedScope;
            if (!parsedScopes.TryGetValue(scope.Id, out parsedScope))
            {
                parsedScope = new ParsedScope { Scope = scope };
                parsedScopes.Add(scope.Id, parsedScope);
            }

            string[] explicitRightNames;
            if (principalRightsPerScope.TryGetValue(scope.Id, out explicitRightNames))
            {
                parsedScope.ExplicitRightNames = parsedScope.ExplicitRightNames.Union(explicitRightNames).ToList();
                parsedScope.RightNames = parsedScope.RightNames.Union(explicitRightNames).ToList();
            }

            if (scope.ParentIds != null)
            {
                foreach (var parentScopeId in scope.ParentIds)
                {
                    ParsedScope parentParsedScope;
                    if (parsedScopes.TryGetValue(parentScopeId, out parentParsedScope))
                    {
                        parsedScope.RightNames = parsedScope.RightNames.Union(parentParsedScope.RightNames).ToList();
                        parentParsedScope.Children.Add(parsedScope);
                    }
                }
            }

            if (scope.ChildIds != null)
            {
                foreach (var childScopeId in scope.ChildIds)
                {
                    Parse(childScopeId, scopes, principalRightsPerScope, parsedScopes);
                }
            }

            parsedScope.RightUnderNames = parsedScope.RightUnderNames.Union(parsedScope.RightNames).ToList();
            foreach (var childParsedScope in parsedScope.Children)
            {
                parsedScope.RightUnderNames = parsedScope.RightUnderNames.Union(childParsedScope.RightUnderNames).ToList();
            }
        }
    }
}
