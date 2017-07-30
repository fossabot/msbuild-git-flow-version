using System;
using JarrodDavis.GitFlowVersion.Core.Configuration;
using JarrodDavis.GitFlowVersion.Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuGet.Versioning;

namespace JarrodDavis.GitFlowVersion.Core.Implementations
{
    internal class VersionResolver : IVersionResolver
    {
        private IBranchCategoryMapper _branchCategoryMapper;
        private ILogger<VersionResolver> _logger;
        private VersionPrereleaseLabelOptions _prereleaseLabels;
        private IVersionResolutionRequestValidator _validator;

        public VersionResolver(IBranchCategoryMapper branchCategoryMapper,
                               ILogger<VersionResolver> logger,
                               IOptions<VersionPrereleaseLabelOptions> options,
                               IVersionResolutionRequestValidator validator)
        {
            _branchCategoryMapper = branchCategoryMapper;
            _logger = logger;
            _prereleaseLabels = options.Value;
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

            if (request.MostRecentStableReleaseVersion is null)
            {
                _logger.LogInformation("No stable release version given; assuming 0.0.0");
                request = new VersionResolutionRequest
                {
                    CurrentBranchName = request.CurrentBranchName,
                    BaseBranchName = request.BaseBranchName,
                    MostRecentStableReleaseVersion = new SemanticVersion(0, 0, 0)
                };
            }

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
                if (baseVersion is null)
                {
                    _logger.LogError("Could not resolve base version for base branch {baseBranch}",
                        request.BaseBranchName);
                    return null;
                }

                _logger.LogDebug("Resolved version {baseVersion} for base branch {baseBranch}",
                    baseVersion, request.BaseBranchName);
                return new SemanticVersion(
                    major: baseVersion.Major,
                    minor: baseVersion.Minor,
                    patch: baseVersion.Patch,
                    releaseLabels: new[] { _prereleaseLabels.AlphaQuality, suffix },
                    metadata: null
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
            if (previousStable is null)
            {
                _logger.LogError("Unexpected null previous version for Beta Quality resolution request");
                return null;
            }

            _logger.LogDebug("Resolving Beta Quality version for previous stable release {previousStable}",
                previousStable);
            return new SemanticVersion(
                major: previousStable.Major,
                minor: previousStable.Minor + 1,
                patch: 0,
                releaseLabel: _prereleaseLabels.BetaQuality
            );
        }

        private SemanticVersion ResolveReleaseCandidate(string suffix)
        {
            _logger.LogDebug("Resolving Release Candidate version for branch suffix {suffix}", suffix);

            if (SemanticVersion.TryParse(suffix, out SemanticVersion version))
            {
                return new SemanticVersion(
                    major: version.Major,
                    minor: version.Minor,
                    patch: version.Patch,
                    releaseLabel: _prereleaseLabels.ReleaseCandidate
                );
            }

            _logger.LogError("Could not parse version from Release Candidate suffix {suffix}", suffix);
            return null;
        }
    }
}
