using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NuGet.Versioning;
using Xunit;

namespace JarrodDavis.GitFlowVersion.Core.Tests
{
    public class VersionResolverTests
    {
        Mock<IBranchCategoryMapper> _branchCategoryMapper;
        Mock<ILogger<VersionResolver>> _logger;
        IOptions<VersionPrereleaseLabelOptions> _options;
        Mock<IVersionResolutionRequestValidator> _validator;

        public VersionResolverTests()
        {
            _branchCategoryMapper = new Mock<IBranchCategoryMapper>();
            _logger = new Mock<ILogger<VersionResolver>>();
            _options = Options.Create(new VersionPrereleaseLabelOptions
            {
                ReleaseCandidate = "rc",
                BetaQuality = "beta",
                AlphaQuality = "alpha"
            });
            _validator = new Mock<IVersionResolutionRequestValidator>();
        }

        [Fact]
        public void ResolverShouldThrowForInvalidArguments()
        {
            // Arrange
            var request = new VersionResolutionRequest();
            _validator.Setup(validator => validator.ValidateRequest(request))
                      .Throws<ArgumentException>();

            var systemUnderTest = ArrangeResolver();

            // Act
            Action action = () => systemUnderTest.ResolveVersion(request);

            // Assert
            action.ShouldThrow<ArgumentException>(because: "the validator threw an exception");
            _validator.VerifyAll();
        }

        [Theory]
        [InlineData("master", "0.1.0")]
        [InlineData("production", "1.0.0")]
        [InlineData("trunk", "1.1.1")]
        public void ResolverShouldResolveStableVersionWithCurrentVersion(string branch,
                                                                         string stableVersionString)
        {
            // Arrange
            var stableVersion = SemanticVersion.Parse(stableVersionString);
            var request = new VersionResolutionRequest
            {
                CurrentBranchName = branch,
                BaseBranchName = null,
                MostRecentStableReleaseVersion = stableVersion,
                CommitsSinceStableRelease = 0
            };

            SetupDependencies(request, BranchCategory.Stable, branchSuffix: null);
            var systemUnderTest = ArrangeResolver();

            // Act
            var resolvedVersion = systemUnderTest.ResolveVersion(request);

            // Assert
            resolvedVersion.Should().Be(request.MostRecentStableReleaseVersion,
                because: $"the current commit on branch {branch} is tagged with version {stableVersion}");
        }

        [Theory]
        [InlineData("release/0.1.0", "0.1.0", null)]
        [InlineData("rel/1.0.0", "1.0.0", "0.1.0")]
        [InlineData("rc/1.1.0", "1.1.0", "1.0.1")]
        public void ResolverShouldResolveReleaseCandidateVersionFromBranchSuffix(
            string branch, string expectedVersionSuffix, string stableVersionString)
        {
            // Arrange
            var expectedVersion = ParseExpectedPrereleaseVersion(expectedVersionSuffix,
                                                                 prereleaseLabel: "rc");

            var request = new VersionResolutionRequest
            {
                CurrentBranchName = branch,
                BaseBranchName = "develop",
                CommitsSinceStableRelease = 20,
                MostRecentStableReleaseVersion = ParseStableVersionString(stableVersionString)
            };

            SetupDependencies(request, BranchCategory.ReleaseCandidate, expectedVersionSuffix);
            var systemUnderTest = ArrangeResolver();

            // Act
            var resolvedVersion = systemUnderTest.ResolveVersion(request);

            // Assert
            resolvedVersion.Should().Be(expectedVersion,
                because: $"the current branch {branch} is a correctly-suffixed Release Candidate branch");
        }

        private SemanticVersion ParseExpectedPrereleaseVersion(string expectedVersionString,
                                                               string prereleaseLabel)
        {
            var expectedVersion = SemanticVersion.Parse(expectedVersionString);
            return new SemanticVersion(
                major: expectedVersion.Major,
                minor: expectedVersion.Minor,
                patch: expectedVersion.Patch,
                releaseLabel: prereleaseLabel
            );
        }

        private SemanticVersion ParseStableVersionString(string stableVersionString) =>
            stableVersionString is null ? null : SemanticVersion.Parse(stableVersionString);

        private void SetupDependencies(VersionResolutionRequest request,
                                       BranchCategory branchCategory,
                                       string branchSuffix)
        {
            _validator.Setup(validator => validator.ValidateRequest(request));
            _branchCategoryMapper.Setup(mapper => mapper.MapBranchName(request.CurrentBranchName))
                                 .Returns((branchCategory, branchSuffix));
        }

        private VersionResolver ArrangeResolver() => new VersionResolver(_branchCategoryMapper.Object,
                                                                         _logger.Object,
                                                                         _options,
                                                                         _validator.Object);
    }
}
