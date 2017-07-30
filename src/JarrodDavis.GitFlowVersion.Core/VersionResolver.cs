using System;
using NuGet.Versioning;

namespace JarrodDavis.GitFlowVersion.Core
{
    public class VersionResolver : IVersionResolver
    {
        private IBranchCategoryMapper _branchCategoryMapper;
        private IVersionResolutionRequestValidator _validator;

        public VersionResolver(IBranchCategoryMapper branchCategoryMapper,
                               IVersionResolutionRequestValidator validator)
        {
            _branchCategoryMapper = branchCategoryMapper;
            _validator = validator;
        }

        public SemanticVersion ResolveVersion(VersionResolutionRequest request)
        {
            _validator.ValidateRequest(request);

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
