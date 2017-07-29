using System;
using NuGet.Versioning;

namespace JarrodDavis.GitFlowVersion.Core
{
    public class VersionResolver : IVersionResolver
    {
        private IBranchCategoryMapper _branchCategoryMapper;

        public VersionResolver(IBranchCategoryMapper branchCategoryMapper)
        {
            _branchCategoryMapper = branchCategoryMapper;
        }

        public SemanticVersion ResolveVersion(VersionResolutionRequest request)
        {
            ValidateRequest(request);

            var match = _branchCategoryMapper.MapBranchName(request.CurrentBranchName);

            switch (match.Category)
            {
                case BranchCategory.Unknown:
                    return null;
                case BranchCategory.AlphaQuality:
                    return ResolveAlphaQuality(request, match.Suffix);
                case BranchCategory.BetaQuality:
                    return ResolveBetaQuality(request.MostRecentStableReleaseVersion);
                case BranchCategory.ReleaseCandidate:
                    return ResolveReleaseCandidate(match.Suffix);
                case BranchCategory.Stable:
                    return request.MostRecentStableReleaseVersion;
                default:
                    throw new NotImplementedException($"Unexpected branch category '${match.Category}'");
            }
        }

        private void ValidateRequest(VersionResolutionRequest request)
        {
            if (request.CurrentBranchName is null)
            {
                throw new ArgumentNullException(nameof(request.CurrentBranchName));
            }

            if (request.BaseBranchName is null)
            {
                throw new ArgumentNullException(nameof(request.BaseBranchName));
            }

            if (request.MostRecentStableReleaseVersion is null)
            {
                throw new ArgumentNullException(nameof(request.MostRecentStableReleaseVersion));
            }

            if (string.IsNullOrWhiteSpace(request.CurrentBranchName))
            {
                throw new ArgumentException("Current branch name cannot be empty or whitespace",
                    nameof(request.CurrentBranchName));
            }

            if (string.IsNullOrWhiteSpace(request.BaseBranchName))
            {
                throw new ArgumentException("Base branch name cannot be empty or whitespace",
                    nameof(request.BaseBranchName));
            }

            if (request.MostRecentStableReleaseVersion.IsPrerelease)
            {
                throw new ArgumentException("Stable release version cannot be a prerelease",
                    nameof(request.MostRecentStableReleaseVersion));
            }

            if (request.CommitsSinceStableRelease < 0)
            {
                throw new ArgumentException("Commits since stable release must be non-negative",
                    nameof(request.CommitsSinceStableRelease));
            }
        }

        private SemanticVersion ResolveAlphaQuality(VersionResolutionRequest request, string suffix)
        {
            var baseMatch = _branchCategoryMapper.MapBranchName(request.BaseBranchName);

            SemanticVersion GenerateAlphaVersion(SemanticVersion baseVersion)
            {
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
                    return null;
                case BranchCategory.BetaQuality:
                    return GenerateAlphaVersion(ResolveBetaQuality(request.MostRecentStableReleaseVersion));
                case BranchCategory.ReleaseCandidate:
                    return GenerateAlphaVersion(ResolveReleaseCandidate(baseMatch.Suffix));
                default:
                    throw new NotImplementedException($"Unexpected branch category '${baseMatch.Category}'");
            }
        }

        private SemanticVersion ResolveBetaQuality(SemanticVersion previousStable)
        {
            return new SemanticVersion(
                major: previousStable.Major,
                minor: previousStable.Minor + 1,
                patch: 0,
                releaseLabel: "beta"
            );
        }

        private SemanticVersion ResolveReleaseCandidate(string suffix)
        {
            return SemanticVersion.TryParse(suffix, out SemanticVersion version) ? version : null;
        }
    }
}
