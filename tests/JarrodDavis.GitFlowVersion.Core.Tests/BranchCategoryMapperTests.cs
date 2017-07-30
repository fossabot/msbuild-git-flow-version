using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace JarrodDavis.GitFlowVersion.Core.Tests
{
    public class BranchCategoryMapperTests
    {
        private BranchCategoryMapper _systemUnderTest;

        public BranchCategoryMapperTests()
        {
            var options = Options.Create(new BranchCategoryMappingOptions
            {
                StableBranchName = "master",
                BetaQualityBranchName = "develop",
                FeatureBranchNamePrefix = "feature/",
                BugfixBranchNamePrefix = "bugfix/",
                ReleaseBranchNamePrefix = "release/",
                HotfixBranchNamePrefix = "hotfix/",
                DetachedHeadName = "DETACHED"
            });

            _systemUnderTest = new BranchCategoryMapper(options);
        }

        [Fact]
        public void MapperShouldMapValidMasterBranchName()
        {
            // Arrange
            const string branchName = "master";

            // Act
            var category = _systemUnderTest.MapBranchName(branchName);

            // Assert
            category.Should().Be((BranchCategory.Stable, null),
                because: $"'{branchName}' is always a Stable branch");
        }

        [Theory]
        [InlineData("release/0.1.0", "0.1.0")]
        [InlineData("release/0.1.1", "0.1.1")]
        [InlineData("release/1.0.0", "1.0.0")]
        [InlineData("release/1.1.0", "1.1.0")]
        [InlineData("release/1.1.1", "1.1.1")]
        public void MapperShouldMapValidReleaseBranchName(string branchName, string expectedSuffix)
        {
            // Arrange

            // Act
            var category = _systemUnderTest.MapBranchName(branchName);

            // Assert
            category.Should().Be((BranchCategory.ReleaseCandidate, expectedSuffix),
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
            category.Should().Be((BranchCategory.Unknown, null),
                because: $"'{branchName}' is not a valid release branch");
        }

        [Theory]
        [InlineData("hotfix/0.1.1", "0.1.1")]
        [InlineData("hotfix/1.0.1", "1.0.1")]
        [InlineData("hotfix/1.1.1", "1.1.1")]
        public void MapperShouldMapValidHotfixBranchName(string branchName, string expectedSuffix)
        {
            // Arrange

            // Act
            var category = _systemUnderTest.MapBranchName(branchName);

            // Assert
            category.Should().Be((BranchCategory.ReleaseCandidate, expectedSuffix),
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
            category.Should().Be((BranchCategory.Unknown, null),
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
            category.Should().Be((BranchCategory.BetaQuality, null),
                because: $"'{branchName}' is always a Beta Quality branch");
        }

        [Theory]
        [InlineData("feature/add-cool-feature", "add-cool-feature")]
        [InlineData("feature/123", "123")]
        [InlineData("feature/PROJ-123", "PROJ-123")]
        [InlineData("feature/do-something-really-really-awesome", "do-something-really-really-awesome")]
        public void MapperShouldMapValidFeatureBranchName(string branchName, string expectedSuffix)
        {
            // Arrange

            // Act
            var category = _systemUnderTest.MapBranchName(branchName);

            // Assert
            category.Should().Be((BranchCategory.AlphaQuality, expectedSuffix),
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
            category.Should().Be((BranchCategory.Unknown, null),
                because: $"'{branchName}' is not a valid feature branch");
        }

        [Theory]
        [InlineData("bugfix/fix-thing", "fix-thing")]
        [InlineData("bugfix/124", "124")]
        [InlineData("bugfix/PROJ-124", "PROJ-124")]
        [InlineData("bugfix/fix-thing-that-is-broken-really-bad", "fix-thing-that-is-broken-really-bad")]
        public void MapperShouldMapValidBugfixBranchName(string branchName, string expectedSuffix)
        {
            // Arrange

            // Act
            var category = _systemUnderTest.MapBranchName(branchName);

            // Assert
            category.Should().Be((BranchCategory.AlphaQuality, expectedSuffix),
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
            category.Should().Be((BranchCategory.Unknown, null),
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
            category.Should().Be((BranchCategory.AlphaQuality, "DETACHED"),
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
            category.Should().Be((BranchCategory.Unknown, null),
                because: $"'{branchName}' is not a valid git-flow branch");
        }
    }
}
