using System;
using FluentAssertions;
using JarrodDavis.GitFlowVersion.Core.Contracts;
using JarrodDavis.GitFlowVersion.Core.Implementations;
using NuGet.Versioning;
using Xunit;

namespace JarrodDavis.GitFlowVersion.Core.Tests
{
    public class VersionResolutionRequestValidatorTests
    {
        private VersionResolutionRequestValidator _systemUnderTest;
        private VersionResolutionRequest _request;

        public VersionResolutionRequestValidatorTests()
        {
            _systemUnderTest = new VersionResolutionRequestValidator();
            _request = new VersionResolutionRequest
            {
                CurrentBranchName = "feature/add-cool-feature",
                BaseBranchName = "develop",
                MostRecentStableReleaseVersion = SemanticVersion.Parse("0.1.0")
            };
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void RequestValidatorShouldThrowForInvalidCurrentBranchName(string branchName)
        {
            // Arrange
            _request.BaseBranchName = branchName;

            // Act
            Action action = () => _systemUnderTest.ValidateRequest(_request);

            // Assert
            action.ShouldThrow<ArgumentException>()
                  .And.ParamName
                  .Should().Be(nameof(_request.BaseBranchName));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void RequestValidatorShouldThrowForInvalidBaseBranchName(string branchName)
        {
            // Arrange
            _request.CurrentBranchName = branchName;

            // Act
            Action action = () => _systemUnderTest.ValidateRequest(_request);

            // Assert
            action.ShouldThrow<ArgumentException>()
                  .And.ParamName
                  .Should().Be(nameof(_request.CurrentBranchName));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("0.1.0-preview4")]
        [InlineData("1.0.0-rc")]
        public void RequestValidatorShouldThrowForInvalidStableVersion(string semanticVersionString)
        {
            // Arrange
            _request.MostRecentStableReleaseVersion = semanticVersionString is null
                ? null
                : SemanticVersion.Parse(semanticVersionString);

            // Act
            Action action = () => _systemUnderTest.ValidateRequest(_request);

            // Assert
            action.ShouldThrow<ArgumentException>()
                  .And.ParamName
                  .Should().Be(nameof(_request.MostRecentStableReleaseVersion));
        }
    }
}
