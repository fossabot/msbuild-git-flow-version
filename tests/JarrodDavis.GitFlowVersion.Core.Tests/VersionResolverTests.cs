using System;
using FluentAssertions;
using NuGet.Versioning;
using Xunit;

namespace JarrodDavis.GitFlowVersion.Core.Tests
{
    public class VersionResolverTests
    {
        private VersionResolver _systemUnderTest;
        private VersionResolutionRequest _request;

        public VersionResolverTests()
        {
            _systemUnderTest = new VersionResolver();
            _request = new VersionResolutionRequest
            {
                CurrentBranchName = "feature/add-cool-feature",
                BaseBranchName = "develop",
                MostRecentStableReleaseVersion = SemanticVersion.Parse("0.1.0"),
                CommitsSinceStableRelease = 4
            };
        }

        [Theory]
        [PairwiseData]
        public void VersionResolverShouldThrowForInvalidArguments(
            [CombinatorialValues(null, "", " ")]
            string currentBranchName,
            [CombinatorialValues(null, "", " ")]
            string baseBranchName,
            [SemanticVersionCombinatorialValues(null, "0.1.0-preview4", "1.0.0-rc")]
            SemanticVersion mostRecentStableReleaseVersion,
            [CombinatorialValues(-10, -5, -1)]
            int commitsSinceStableVersion
        )
        {
            // Arrange
            var request = new VersionResolutionRequest
            {
                CurrentBranchName = currentBranchName,
                BaseBranchName = baseBranchName,
                MostRecentStableReleaseVersion = mostRecentStableReleaseVersion,
                CommitsSinceStableRelease = commitsSinceStableVersion
            };

            // Act
            Action action = () => _systemUnderTest.ResolveVersion(request);

            // Assert
            action.ShouldThrow<ArgumentException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void VersionResolverShouldThrowForInvalidCurrentBranchName(string branchName)
        {
            // Arrange
            _request.BaseBranchName = branchName;

            // Act
            Action action = () => _systemUnderTest.ResolveVersion(_request);

            // Assert
            action.ShouldThrow<ArgumentException>()
                  .And.ParamName
                  .Should().Be(nameof(_request.BaseBranchName));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void VersionResolverShouldThrowForInvalidBaseBranchName(string branchName)
        {
            // Arrange
            _request.CurrentBranchName = branchName;

            // Act
            Action action = () => _systemUnderTest.ResolveVersion(_request);

            // Assert
            action.ShouldThrow<ArgumentException>()
                  .And.ParamName
                  .Should().Be(nameof(_request.CurrentBranchName));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("0.1.0-preview4")]
        [InlineData("1.0.0-rc")]
        public void VersionResolverShouldThrowForInvalidStableVersion(string semanticVersionString)
        {
            // Arrange
            _request.MostRecentStableReleaseVersion = semanticVersionString is null
                ? null
                : SemanticVersion.Parse(semanticVersionString);

            // Act
            Action action = () => _systemUnderTest.ResolveVersion(_request);

            // Assert
            action.ShouldThrow<ArgumentException>()
                  .And.ParamName
                  .Should().Be(nameof(_request.MostRecentStableReleaseVersion));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        [InlineData(-10)]
        public void VersionResolverShouldThrowForInvalidCommitCount(int commitCount)
        {
            // Arrange
            _request.CommitsSinceStableRelease = commitCount;

            // Act
            Action action = () => _systemUnderTest.ResolveVersion(_request);

            // Assert
            action.ShouldThrow<ArgumentException>()
                  .And.ParamName
                  .Should().Be(nameof(_request.CommitsSinceStableRelease));
        }
    }
}
