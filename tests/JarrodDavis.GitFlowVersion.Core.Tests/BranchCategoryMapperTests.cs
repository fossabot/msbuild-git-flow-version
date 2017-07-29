using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JarrodDavis.GitFlowVersion.Core.Tests
{
    public class BranchCategoryMapperTests
    {
        private BranchCategoryMapper _systemUnderTest;

        public BranchCategoryMapperTests()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddOptions()
                .Configure<BranchCategoryMappingOptions>(configuration.GetSection("BranchCategoryMapper"))
                .AddSingleton<BranchCategoryMapper>()
                .BuildServiceProvider();

            _systemUnderTest = serviceProvider.GetRequiredService<BranchCategoryMapper>();
        }

        [Fact]
        public void MapperShouldMapValidMasterBranchName()
        {
            // Arrange
            const string branchName = "master";

            // Act
            var category = _systemUnderTest.MapBranchName(branchName);

            // Assert
            category.Should().Be(BranchCategory.Stable,
                because: $"'{branchName}' is always a Stable branch");
        }

        [Theory]
        [InlineData("release/0.1.0")]
        [InlineData("release/0.1.1")]
        [InlineData("release/1.0.0")]
        [InlineData("release/1.1.0")]
        [InlineData("release/1.1.1")]
        public void MapperShouldMapValidReleaseBranchName(string branchName)
        {
            // Arrange

            // Act
            var category = _systemUnderTest.MapBranchName(branchName);

            // Assert
            category.Should().Be(BranchCategory.ReleaseCandidate,
                because: $"'{branchName}' is a valid release branch");
        }

        [Theory]
        [InlineData("release")]
        [InlineData("release/")]
        [InlineData("release/fantastic-release")]
        [InlineData("release/-1")]
        [InlineData("release/0.1.0-preview4")]
        [InlineData("release/0")]
        [InlineData("release/0.1")]
        [InlineData("release/1.1.0.4")]
        public void MapperShouldNotMapInvalidReleaseBranchName(string branchName)
        {
            // Arrange

            // Act
            var category = _systemUnderTest.MapBranchName(branchName);

            // Assert
            category.Should().Be(BranchCategory.Unknown,
                because: $"'{branchName}' is not a valid release branch");
        }

        [Theory]
        [InlineData("hotfix/0.1.1")]
        [InlineData("hotfix/1.0.1")]
        [InlineData("hotfix/1.1.1")]
        public void MapperShouldMapValidHotfixBranchName(string branchName)
        {
            // Arrange

            // Act
            var category = _systemUnderTest.MapBranchName(branchName);

            // Assert
            category.Should().Be(BranchCategory.ReleaseCandidate,
                because: $"'{branchName} is a valid hotfix branch");
        }

        [Theory]
        [InlineData("hotfix")]
        [InlineData("hotfix/")]
        [InlineData("hotfix/quick-fix")]
        [InlineData("hotfix/-2")]
        [InlineData("hotfix/0.1.1-rc")]
        [InlineData("hotfix/0")]
        [InlineData("hotfix/0.1")]
        [InlineData("hotfix/1.0.1.1")]
        public void MapperShouldNotMapInvalidHotfixBranchName(string branchName)
        {
            // Arrange

            // Act
            var category = _systemUnderTest.MapBranchName(branchName);

            // Assert
            category.Should().Be(BranchCategory.Unknown,
                because: $"'{branchName}' is not a valid hotfix branch");
        }

        [Fact]
        public void MapperShouldMapValidDevelopBranchName()
        {
            // Arrange
            const string branchName = "develop";

            // Act
            var category = _systemUnderTest.MapBranchName(branchName);

            // Assert
            category.Should().Be(BranchCategory.BetaQuality,
                because: $"'{branchName}' is always a Beta Quality branch");
        }

        [Theory]
        [InlineData("feature/add-cool-feature")]
        [InlineData("feature/123")]
        [InlineData("feature/PROJ-123")]
        [InlineData("feature/do-something-really-really-awesome")]
        public void MapperShouldMapValidFeatureBranchName(string branchName)
        {
            // Arrange

            // Act
            var category = _systemUnderTest.MapBranchName(branchName);

            // Assert
            category.Should().Be(BranchCategory.AlphaQuality,
                because: $"'{branchName}' is a valid feature branch");
        }

        [Theory]
        [InlineData("feature")]
        [InlineData("feature/")]
        [InlineData("feature/.")]
        [InlineData("feature/.add-cool-feature")]
        [InlineData("feature/add.cool-feature")]
        [InlineData("feature/+")]
        [InlineData("feature/+add-cool-feature")]
        [InlineData("feature/add+cool-feature")]
        public void MapperShouldNotMapInvalidFeatureBranchName(string branchName)
        {
            // Arrange

            // Act
            var category = _systemUnderTest.MapBranchName(branchName);

            // Assert
            category.Should().Be(BranchCategory.Unknown,
                because: $"'{branchName}' is not a valid feature branch");
        }

        [Theory]
        [InlineData("bugfix/fix-thing")]
        [InlineData("bugfix/124")]
        [InlineData("bugfix/PROJ-123")]
        [InlineData("bugfix/fix-thing-that-is-broken-really-bad")]
        public void MapperShouldMapValidBugfixBranchName(string branchName)
        {
            // Arrange

            // Act
            var category = _systemUnderTest.MapBranchName(branchName);

            // Assert
            category.Should().Be(BranchCategory.AlphaQuality,
                because: $"'{branchName}' is a valid bugfix branch");
        }

        [Theory]
        [InlineData("bugfix")]
        [InlineData("bugfix/")]
        [InlineData("bugfix/.")]
        [InlineData("bugfix/.fix-thing")]
        [InlineData("bugfix/fix.thing")]
        [InlineData("bugfix/+")]
        [InlineData("bugfix/+fix-thing")]
        [InlineData("bugfix/fix+thing")]
        public void MapperShouldNotMapInvalidBugfixBranchName(string branchName)
        {
            // Arrange

            // Act
            var category = _systemUnderTest.MapBranchName(branchName);

            // Assert
            category.Should().Be(BranchCategory.Unknown,
                because: $"'{branchName}' is not a valid bugfix branch");
        }

        [Fact]
        public void MapperShouldMapDetachedHead()
        {
            // Arrange
            const string branchName = "DETACHED";

            // Act
            var category = _systemUnderTest.MapBranchName(branchName);

            // Assert
            category.Should().Be(BranchCategory.AlphaQuality,
                because: $"Detached HEAD should always be mapped to Alpha Quality");
        }

        [Theory]
        [InlineData("master_old")]
        [InlineData("develop/something")]
        [InlineData("some-cool-feature")]
        [InlineData("2017/some-new-feature")]
        public void MapperShouldNotMapInvalidBranchName(string branchName)
        {
            // Arrange

            // Act
            var category = _systemUnderTest.MapBranchName(branchName);

            // Assert
            category.Should().Be(BranchCategory.Unknown,
                because: $"'{branchName}' is not a valid git-flow branch");
        }
    }
}
