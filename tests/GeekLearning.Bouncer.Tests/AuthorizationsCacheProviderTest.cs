namespace GeekLearning.Bouncer.Tests
{
    using Bouncer.EntityFrameworkCore;
    using Bouncer.EntityFrameworkCore.Caching;
    using Bouncer.EntityFrameworkCore.Data;
    using Bouncer.EntityFrameworkCore.Exceptions;
    using GeekLearning.Bouncer.EntityFrameworkCore.Data.Extensions;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class AuthorizationsCacheProviderTest
    {
        //[Fact]
        public async Task ScopeModelValidation_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                var scope1Entity = bouncerFixture.Context.Scopes.Add(new EntityFrameworkCore.Data.Scope
                {
                    Id = Guid.NewGuid(),
                    Name = "scope1",
                    Description = "scope 1",
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId,
                });

                var scope2Entity = bouncerFixture.Context.Scopes.Add(new EntityFrameworkCore.Data.Scope
                {
                    Id = Guid.NewGuid(),
                    Name = "scope2",
                    Description = "scope 2",
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId,
                });

                var scope3Entity = bouncerFixture.Context.Scopes.Add(new EntityFrameworkCore.Data.Scope
                {
                    Id = Guid.NewGuid(),
                    Name = "scope3",
                    Description = "scope 3",
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId,
                });

                var scope4Entity = bouncerFixture.Context.Scopes.Add(new EntityFrameworkCore.Data.Scope
                {
                    Id = Guid.NewGuid(),
                    Name = "scope4",
                    Description = "scope 4",
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId,
                });

                var scope5Entity = bouncerFixture.Context.Scopes.Add(new EntityFrameworkCore.Data.Scope
                {
                    Id = Guid.NewGuid(),
                    Name = "scope5",
                    Description = "scope 5",
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId,
                });

                var scope6Entity = bouncerFixture.Context.Scopes.Add(new EntityFrameworkCore.Data.Scope
                {
                    Id = Guid.NewGuid(),
                    Name = "scope6",
                    Description = "scope 6",
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId,
                });

                var scope7Entity = bouncerFixture.Context.Scopes.Add(new EntityFrameworkCore.Data.Scope
                {
                    Id = Guid.NewGuid(),
                    Name = "scope7",
                    Description = "scope 7",
                    CreationBy = bouncerFixture.Context.CurrentUserId,
                    ModificationBy = bouncerFixture.Context.CurrentUserId,
                });

                bouncerFixture.Context.ScopeHierarchies.Add(new ScopeHierarchy
                {
                    Child = scope6Entity.Entity,
                    Parent = scope3Entity.Entity,
                });

                bouncerFixture.Context.ScopeHierarchies.Add(new ScopeHierarchy
                {
                    Child = scope6Entity.Entity,
                    Parent = scope5Entity.Entity,
                });

                bouncerFixture.Context.ScopeHierarchies.Add(new ScopeHierarchy
                {
                    Child = scope3Entity.Entity,
                    Parent = scope2Entity.Entity,
                });

                bouncerFixture.Context.ScopeHierarchies.Add(new ScopeHierarchy
                {
                    Child = scope2Entity.Entity,
                    Parent = scope1Entity.Entity,
                });

                bouncerFixture.Context.ScopeHierarchies.Add(new ScopeHierarchy
                {
                    Child = scope5Entity.Entity,
                    Parent = scope4Entity.Entity,
                });

                bouncerFixture.Context.ScopeHierarchies.Add(new ScopeHierarchy
                {
                    Child = scope4Entity.Entity,
                    Parent = scope1Entity.Entity,
                });

                bouncerFixture.Context.ScopeHierarchies.Add(new ScopeHierarchy
                {
                    Child = scope7Entity.Entity,
                    Parent = scope6Entity.Entity,
                });

                bouncerFixture.Context.ScopeHierarchies.Add(new ScopeHierarchy
                {
                    Child = scope2Entity.Entity,
                    Parent = scope7Entity.Entity,
                });

                await bouncerFixture.Context.SaveChangesAsync();

                var authorizationsCacheProvider = new AuthorizationsCacheProvider(bouncerFixture.Context);

                await Assert.ThrowsAsync<BadScopeModelConfigurationException>(async () => await authorizationsCacheProvider.GetScopesAsync(s => s.Id));                
            }
        }
    }
}
