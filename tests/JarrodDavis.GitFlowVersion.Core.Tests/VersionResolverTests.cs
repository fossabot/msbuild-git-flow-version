using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JarrodDavis.GitFlowVersion.Core.Tests
{
    public class VersionResolverTests
    {
        Mock<IBranchCategoryMapper> _branchCategoryMapper;
        Mock<ILogger<VersionResolver>> _logger;
        Mock<IVersionResolutionRequestValidator> _validator;

        public VersionResolverTests()
        {
            _branchCategoryMapper = new Mock<IBranchCategoryMapper>();
            _logger = new Mock<ILogger<VersionResolver>>();
            _validator = new Mock<IVersionResolutionRequestValidator>();
        }

        [Fact]
        public void ResolverShouldThrowForInvalidArguments()
        {
            // Arrange
            var request = new VersionResolutionRequest();
            _validator.Setup(validator => validator.ValidateRequest(request))
                      .Throws<ArgumentException>();
            var systemUnderTest = new VersionResolver(_branchCategoryMapper.Object,
                                                      _logger.Object,
                                                      _validator.Object);

            // Act
            Action action = () => systemUnderTest.ResolveVersion(request);

            // Assert
            action.ShouldThrow<ArgumentException>(because: "the validator threw an exception");
            _validator.VerifyAll();
        }
    }
}
