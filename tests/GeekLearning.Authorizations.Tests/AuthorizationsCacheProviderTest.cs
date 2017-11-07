namespace GeekLearning.Authorizations.Tests
{
    using GeekLearning.Authorizations.EntityFrameworkCore;
    using GeekLearning.Authorizations.EntityFrameworkCore.Caching;
    using GeekLearning.Authorizations.EntityFrameworkCore.Data;
    using GeekLearning.Authorizations.EntityFrameworkCore.Exceptions;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class AuthorizationsCacheProviderTest
    {
        [Fact]
        public async Task ScopeModelValidation_ShouldBeOk()
        {
            using (var authorizationsFixture = new AuthorizationsFixture())
            {
                var scope1Entity = authorizationsFixture.Context.Scopes().Add(new EntityFrameworkCore.Data.Scope
                {
                    Id = Guid.NewGuid(),
                    Name = "scope1",
                    Description = "scope 1",
                    CreationBy = authorizationsFixture.Context.CurrentUserId,
                    ModificationBy = authorizationsFixture.Context.CurrentUserId,
                });

                var scope2Entity = authorizationsFixture.Context.Scopes().Add(new EntityFrameworkCore.Data.Scope
                {
                    Id = Guid.NewGuid(),
                    Name = "scope2",
                    Description = "scope 2",
                    CreationBy = authorizationsFixture.Context.CurrentUserId,
                    ModificationBy = authorizationsFixture.Context.CurrentUserId,
                });

                var scope3Entity = authorizationsFixture.Context.Scopes().Add(new EntityFrameworkCore.Data.Scope
                {
                    Id = Guid.NewGuid(),
                    Name = "scope3",
                    Description = "scope 3",
                    CreationBy = authorizationsFixture.Context.CurrentUserId,
                    ModificationBy = authorizationsFixture.Context.CurrentUserId,
                });

                var scope4Entity = authorizationsFixture.Context.Scopes().Add(new EntityFrameworkCore.Data.Scope
                {
                    Id = Guid.NewGuid(),
                    Name = "scope4",
                    Description = "scope 4",
                    CreationBy = authorizationsFixture.Context.CurrentUserId,
                    ModificationBy = authorizationsFixture.Context.CurrentUserId,
                });

                var scope5Entity = authorizationsFixture.Context.Scopes().Add(new EntityFrameworkCore.Data.Scope
                {
                    Id = Guid.NewGuid(),
                    Name = "scope5",
                    Description = "scope 5",
                    CreationBy = authorizationsFixture.Context.CurrentUserId,
                    ModificationBy = authorizationsFixture.Context.CurrentUserId,
                });

                var scope6Entity = authorizationsFixture.Context.Scopes().Add(new EntityFrameworkCore.Data.Scope
                {
                    Id = Guid.NewGuid(),
                    Name = "scope6",
                    Description = "scope 6",
                    CreationBy = authorizationsFixture.Context.CurrentUserId,
                    ModificationBy = authorizationsFixture.Context.CurrentUserId,
                });

                var scope7Entity = authorizationsFixture.Context.Scopes().Add(new EntityFrameworkCore.Data.Scope
                {
                    Id = Guid.NewGuid(),
                    Name = "scope7",
                    Description = "scope 7",
                    CreationBy = authorizationsFixture.Context.CurrentUserId,
                    ModificationBy = authorizationsFixture.Context.CurrentUserId,
                });

                authorizationsFixture.Context.ScopeHierarchies().Add(new ScopeHierarchy
                {
                    Child = scope6Entity.Entity,
                    Parent = scope3Entity.Entity,
                });

                authorizationsFixture.Context.ScopeHierarchies().Add(new ScopeHierarchy
                {
                    Child = scope6Entity.Entity,
                    Parent = scope5Entity.Entity,
                });

                authorizationsFixture.Context.ScopeHierarchies().Add(new ScopeHierarchy
                {
                    Child = scope3Entity.Entity,
                    Parent = scope2Entity.Entity,
                });

                authorizationsFixture.Context.ScopeHierarchies().Add(new ScopeHierarchy
                {
                    Child = scope2Entity.Entity,
                    Parent = scope1Entity.Entity,
                });

                authorizationsFixture.Context.ScopeHierarchies().Add(new ScopeHierarchy
                {
                    Child = scope5Entity.Entity,
                    Parent = scope4Entity.Entity,
                });

                authorizationsFixture.Context.ScopeHierarchies().Add(new ScopeHierarchy
                {
                    Child = scope4Entity.Entity,
                    Parent = scope1Entity.Entity,
                });

                authorizationsFixture.Context.ScopeHierarchies().Add(new ScopeHierarchy
                {
                    Child = scope7Entity.Entity,
                    Parent = scope6Entity.Entity,
                });

                authorizationsFixture.Context.ScopeHierarchies().Add(new ScopeHierarchy
                {
                    Child = scope2Entity.Entity,
                    Parent = scope7Entity.Entity,
                });

                await authorizationsFixture.Context.SaveChangesAsync();

                var authorizationsCacheProvider = new AuthorizationsCacheProvider<AuthorizationsTestContext>(authorizationsFixture.Context);

                await Assert.ThrowsAsync<BadScopeModelConfigurationException>(async () => await authorizationsCacheProvider.GetScopesAsync());                
            }
        }
    }
}
