namespace GeekLearning.Bouncer.Tests
{
    using EntityFrameworkCore;
    using EntityFrameworkCore.Data;
    using GeekLearning.Bouncer.EntityFrameworkCore.Data.Extensions;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class RightTests
    {
        [Fact]
        public async Task CreateRight_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                await bouncerFixture.AuthorizationsManager.CreateRightAsync("right1");

                await bouncerFixture.Context.SaveChangesAsync();

                Assert.NotNull(bouncerFixture.Context.Rights.FirstOrDefault(r => r.Name == "right1"));
            }
        }

        [Fact]
        public async Task DeleteRight_ShouldBeOk()
        {
            using (var bouncerFixture = new BouncerFixture())
            {
                bouncerFixture.Context.Rights.Add(
                    new Right
                    {
                        Name = "right1",
                        CreationBy = bouncerFixture.Context.CurrentUserId,
                        ModificationBy = bouncerFixture.Context.CurrentUserId
                    });

                await bouncerFixture.Context.SaveChangesAsync();

                await bouncerFixture.AuthorizationsManager.DeleteRightAsync("right1");

                await bouncerFixture.Context.SaveChangesAsync();

                Assert.Null(bouncerFixture.Context.Rights.FirstOrDefault(r => r.Name == "right1"));
            }
        }
    }
}
