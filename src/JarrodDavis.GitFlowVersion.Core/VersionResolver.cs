using System;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

namespace JarrodDavis.GitFlowVersion.Core
{
    public class VersionResolver : IVersionResolver
    {
        private IBranchCategoryMapper _branchCategoryMapper;
        private ILogger<VersionResolver> _logger;
        private IVersionResolutionRequestValidator _validator;

        public VersionResolver(IBranchCategoryMapper branchCategoryMapper,
                               ILogger<VersionResolver> logger,
                               IVersionResolutionRequestValidator validator)
        {
            _branchCategoryMapper = branchCategoryMapper;
            _logger = logger;
            _validator = validator;
        }

        public SemanticVersion ResolveVersion(VersionResolutionRequest request)
        {
            _logger.LogDebug("Validating version resolution request");
            _validator.ValidateRequest(request);
            _logger.LogDebug("Version resolution request validated successfully");

            _logger.LogDebug("Finding category for current branch {currentBranch}",
                request.CurrentBranchName);
            var match = _branchCategoryMapper.MapBranchName(request.CurrentBranchName);

            var version = ResolveVersionCore(request, match);

            _logger.LogInformation("Resolved version {version} for current branch {currentBranch}",
                version, request.CurrentBranchName);

            return version;
        }

        private SemanticVersion ResolveVersionCore(VersionResolutionRequest request,
                                                   (BranchCategory Category, string Suffix) match)
        {
            switch (match.Category)
            {
                case BranchCategory.Unknown:
                    _logger.LogError("Current branch {currentBranch} could not be mapped",
                        request.CurrentBranchName);
                    return null;
                case BranchCategory.AlphaQuality:
                    _logger.LogInformation(
                        "Current branch {currentBranch} is an Alpha Quality branch with suffix {suffix}",
                        request.CurrentBranchName, match.Suffix);
                    return ResolveAlphaQuality(request, match.Suffix);
                case BranchCategory.BetaQuality:
                    _logger.LogInformation("Current branch {currentBranch} is a Beta Quality branch",
                        request.CurrentBranchName);
                    return ResolveBetaQuality(request.MostRecentStableReleaseVersion);
                case BranchCategory.ReleaseCandidate:
                    _logger.LogInformation(
                        "Current branch {currentBranch} is a Release Candidate branch with suffix {suffix}",
                        request.CurrentBranchName, match.Suffix);
                    return ResolveReleaseCandidate(match.Suffix);
                case BranchCategory.Stable:
                    _logger.LogInformation(
                        "Current branch {currentBranch} is a Stable branch with version {version}",
                        request.CurrentBranchName, request.MostRecentStableReleaseVersion);
                    return new SemanticVersion(request.MostRecentStableReleaseVersion);
                default:
                    throw new NotImplementedException($"Unexpected branch category '${match.Category}'");
            }
        }

        private SemanticVersion ResolveAlphaQuality(VersionResolutionRequest request, string suffix)
        {
            _logger.LogDebug("Resolving Alpha Quality version for current branch {currentBranch}",
                request.CurrentBranchName);

            _logger.LogDebug("Finding category for base branch {baseBranch}", request.BaseBranchName);
            var baseMatch = _branchCategoryMapper.MapBranchName(request.BaseBranchName);

            SemanticVersion GenerateAlphaVersion(SemanticVersion baseVersion)
            {
                _logger.LogDebug("Resolved version {baseVersion} for base branch {baseBranch}",
                    baseVersion, request.BaseBranchName);
                return new SemanticVersion(
                    major: baseVersion.Major,
                    minor: baseVersion.Minor,
                    patch: baseVersion.Patch,
                    releaseLabels: new[] { "alpha", suffix },
                    metadata: $"commits.{request.CommitsSinceStableRelease}"
                );
            }

            switch (baseMatch.Category)
            {
                case BranchCategory.Unknown:
                case BranchCategory.AlphaQuality:
                case BranchCategory.Stable:
                    _logger.LogError("Unexpected category {category} for base branch {baseBranch}",
                        baseMatch.Category, request.BaseBranchName);
                    return null;
                case BranchCategory.BetaQuality:
                    _logger.LogInformation("Base branch {baseBranch} is a Beta Quality branch",
                        request.BaseBranchName);
                    return GenerateAlphaVersion(ResolveBetaQuality(request.MostRecentStableReleaseVersion));
                case BranchCategory.ReleaseCandidate:
                    _logger.LogInformation("Base branch {baseBranch} is a Release Candidate branch",
                        request.BaseBranchName);
                    return GenerateAlphaVersion(ResolveReleaseCandidate(baseMatch.Suffix));
                default:
                    throw new NotImplementedException($"Unexpected branch category '${baseMatch.Category}'");
            }
        }

        private SemanticVersion ResolveBetaQuality(SemanticVersion previousStable)
        {
            _logger.LogDebug("Resolving Beta Quality version for previous stable release {previousStable}",
                previousStable);
            return new SemanticVersion(
                major: previousStable.Major,
                minor: previousStable.Minor + 1,
                patch: 0,
                releaseLabel: "beta"
            );
        }

        private SemanticVersion ResolveReleaseCandidate(string suffix)
        {
            _logger.LogDebug("Resolving Release Candidate version for branch suffix {suffix}", suffix);
            return SemanticVersion.TryParse(suffix, out SemanticVersion version) ? version : null;
        }
    }
}
