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
                MostRecentStableReleaseVersion = stableVersion
            };

            SetupDependencies(request, BranchCategory.Stable);
            var systemUnderTest = ArrangeResolver();

            // Act
            var resolvedVersion = systemUnderTest.ResolveVersion(request);

            // Assert
            resolvedVersion.Should().Be(request.MostRecentStableReleaseVersion,
                because: $"the current commit on branch {branch} is tagged with version {stableVersion}");
            VerifyAllMocks();
        }

        [Theory]
        [InlineData("release/0.1.0", "0.1.0", null)]
        [InlineData("hotfix/0.1.1", "0.1.1", "0.1.0")]
        [InlineData("rel/1.0.0", "1.0.0", "0.1.1")]
        [InlineData("quickfix/1.0.1", "1.0.1", "1.0.0")]
        [InlineData("rc/1.1.0", "1.1.0", "1.0.1")]
        [InlineData("hf/1.1.1", "1.1.1", "1.1.0")]
        public void ResolverShouldResolveReleaseCandidateVersionFromBranchSuffix(
            string branch, string expectedVersionSuffix, string stableVersionString)
        {
            // Arrange
            var expectedVersion = ParseExpectedPrereleaseVersion(expectedVersionSuffix, "rc");

            var request = new VersionResolutionRequest
            {
                CurrentBranchName = branch,
                BaseBranchName = "develop",
                MostRecentStableReleaseVersion = ParseStableVersionString(stableVersionString)
            };

            SetupDependencies(request, BranchCategory.ReleaseCandidate, expectedVersionSuffix);
            var systemUnderTest = ArrangeResolver();

            // Act
            var resolvedVersion = systemUnderTest.ResolveVersion(request);

            // Assert
            resolvedVersion.Should().Be(expectedVersion,
                because: $"the current branch {branch} is a correctly-suffixed Release Candidate branch");
            VerifyAllMocks();
        }

        [Theory]
        [InlineData("develop", null, "0.1.0")]
        [InlineData("dev", "0.1.0", "0.2.0")]
        [InlineData("next", "0.1.1", "0.2.0")]
        [InlineData("develop", "1.0.0", "1.1.0")]
        [InlineData("dev", "1.0.1", "1.1.0")]
        [InlineData("next", "1.1.0", "1.2.0")]
        [InlineData("develop", "1.1.1", "1.2.0")]
        public void ResolverShouldResolveBetaQualityVersionFromPreviousStableReleaseIncrement(
            string branch, string stableVersionString, string expectedVersionString)
        {
            // Arrange
            var expectedVersion = ParseExpectedPrereleaseVersion(expectedVersionString, "beta");
            var stableVersion = ParseStableVersionString(stableVersionString);
            var request = new VersionResolutionRequest
            {
                CurrentBranchName = branch,
                BaseBranchName = "master",
                MostRecentStableReleaseVersion = stableVersion
            };

            SetupDependencies(request, BranchCategory.BetaQuality);
            var systemUnderTest = ArrangeResolver();

            // Act
            var resolvedVersion = systemUnderTest.ResolveVersion(request);

            // Assert
            resolvedVersion.Should().Be(expectedVersion,
                "because branch {0} should be strictly minor-version incremented from stable version {1}",
                branch, stableVersion);
        }

        [Theory]
        [InlineData("feature/add-cool-feature", null, "0.1.0", "add-cool-feature")]
        [InlineData("feature/add-cool-feature", "0.1.0", "0.2.0", "add-cool-feature")]
        [InlineData("feature/add-cool-feature", "0.1.1", "0.2.0", "add-cool-feature")]
        [InlineData("feature/add-cool-feature", "1.0.0", "1.1.0", "add-cool-feature")]
        [InlineData("feature/add-cool-feature", "1.0.1", "1.1.0", "add-cool-feature")]
        [InlineData("feature/add-cool-feature", "1.1.0", "1.2.0", "add-cool-feature")]
        [InlineData("feature/add-cool-feature", "1.1.1", "1.2.0", "add-cool-feature")]
        [InlineData("bugfix/fix-thing", null, "0.1.0", "fix-thing")]
        [InlineData("bugfix/fix-thing", "0.1.0", "0.2.0", "fix-thing")]
        [InlineData("bugfix/fix-thing", "0.1.1", "0.2.0", "fix-thing")]
        [InlineData("bugfix/fix-thing", "1.0.0", "1.1.0", "fix-thing")]
        [InlineData("bugfix/fix-thing", "1.0.1", "1.1.0", "fix-thing")]
        [InlineData("bugfix/fix-thing", "1.1.0", "1.2.0", "fix-thing")]
        [InlineData("bugfix/fix-thing", "1.1.1", "1.2.0", "fix-thing")]
        public void ResolverShouldResolveAlphaQualityVersionFromBetaQualityBase(
            string branch, string stableVersionString, string expectedVersionString, string branchSuffix)
        {
            // Arrange
            var expectedVersion = ParseExpectedPrereleaseVersion(
                expectedVersionString, "alpha", branchSuffix);
            var stableVersion = ParseStableVersionString(stableVersionString);
            var request = new VersionResolutionRequest
            {
                CurrentBranchName = branch,
                BaseBranchName = "develop",
                MostRecentStableReleaseVersion = stableVersion
            };

            SetupDependencies(request, BranchCategory.AlphaQuality, branchSuffix);
            _branchCategoryMapper.Setup(mapper => mapper.MapBranchName("develop"))
                                 .Returns((BranchCategory.BetaQuality, null));
            var systemUnderTest = ArrangeResolver();

            // Act
            var resolvedVersion = systemUnderTest.ResolveVersion(request);

            // Assert
            resolvedVersion.Should().Be(expectedVersion,
                "because branch {0} should be strictly minor-version incremented from stable version {1}",
                branch, stableVersion);
            VerifyAllMocks();
        }

        private SemanticVersion ParseExpectedPrereleaseVersion(string expectedVersionString,
                                                               params string[] prereleaseLabels)
        {
            var expectedVersion = SemanticVersion.Parse(expectedVersionString);
            return new SemanticVersion(
                major: expectedVersion.Major,
                minor: expectedVersion.Minor,
                patch: expectedVersion.Patch,
                releaseLabels: prereleaseLabels,
                metadata: null
            );
        }

        private SemanticVersion ParseStableVersionString(string stableVersionString) =>
            stableVersionString is null ? null : SemanticVersion.Parse(stableVersionString);

        private void SetupDependencies(VersionResolutionRequest request,
                                       BranchCategory branchCategory,
                                       string branchSuffix = null)
        {
            _validator.Setup(validator => validator.ValidateRequest(request));
            _branchCategoryMapper.Setup(mapper => mapper.MapBranchName(request.CurrentBranchName))
                                 .Returns((branchCategory, branchSuffix));
        }

        private VersionResolver ArrangeResolver() => new VersionResolver(_branchCategoryMapper.Object,
                                                                         _logger.Object,
                                                                         _options,
                                                                         _validator.Object);

        private void VerifyAllMocks()
        {
            _validator.VerifyAll();
            _branchCategoryMapper.VerifyAll();
        }
    }
}
