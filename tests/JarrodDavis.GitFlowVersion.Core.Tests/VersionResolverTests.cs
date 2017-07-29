using System;
using FluentAssertions;
using NuGet.Versioning;
using Xunit;

namespace JarrodDavis.GitFlowVersion.Core.Tests
{
    public class VersionResolverTests
    {
        private VersionResolver _systemUnderTest;

        public VersionResolverTests()
        {
            _systemUnderTest = new VersionResolver();
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
            int commitsSincePreviousStableVersion
        )
        {
            // Arrange
            var request = new VersionResolutionRequest
            {
                CurrentBranchName = currentBranchName,
                BaseBranchName = baseBranchName,
                MostRecentStableReleaseVersion = mostRecentStableReleaseVersion,
                CommitsSincePreviousStableRelease = commitsSincePreviousStableVersion
            };

            // Act
            Action action = () => _systemUnderTest.ResolveVersion(request);

            // Assert
            action.ShouldThrow<ArgumentException>();
        }
    }
}
