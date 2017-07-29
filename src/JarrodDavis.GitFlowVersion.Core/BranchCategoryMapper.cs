using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using NuGet.Versioning;
using static JarrodDavis.GitFlowVersion.Core.BranchCategory;

namespace JarrodDavis.GitFlowVersion.Core
{
    public class BranchCategoryMapper : IBranchCategoryMapper
    {
        // Only alphanumeric (and the hyphen) characters are allowed
        private static readonly Regex AlphaQualitySuffixPattern = new Regex("^[a-zA-Z0-9-]+$");

        private BranchCategoryMappingOptions _options;

        public BranchCategoryMapper(IOptions<BranchCategoryMappingOptions> options)
        {
            _options = options.Value;
        }

        public BranchCategory MapBranchName(string branchName)
        {
            if (branchName is null)
            {
                throw new ArgumentNullException(nameof(branchName));
            }

            if (string.IsNullOrWhiteSpace(branchName))
            {
                throw new ArgumentException("Branch name cannot be empty or whitespace", nameof(branchName));
            }

            if (branchName == _options.StableBranchName)
            {
                return Stable;
            }

            if (branchName.StartsWith(_options.ReleaseBranchNamePrefix))
            {
                var suffix = GetBranchSuffix(branchName, _options.ReleaseBranchNamePrefix);
                return IsValidReleaseCandidateSuffix(suffix) ? ReleaseCandidate : Unknown;
            }

            if (branchName.StartsWith(_options.HotfixBranchNamePrefix))
            {
                var suffix = GetBranchSuffix(branchName, _options.HotfixBranchNamePrefix);
                return IsValidReleaseCandidateSuffix(suffix) ? ReleaseCandidate : Unknown;
            }

            if (branchName == _options.BetaQualityBranchName)
            {
                return BetaQuality;
            }

            if (branchName.StartsWith(_options.FeatureBranchNamePrefix))
            {
                var suffix = GetBranchSuffix(branchName, _options.FeatureBranchNamePrefix);
                return IsValidAlphaQualitySuffix(suffix) ? AlphaQuality : Unknown;
            }

            if (branchName.StartsWith(_options.BugfixBranchNamePrefix))
            {
                var suffix = GetBranchSuffix(branchName, _options.BugfixBranchNamePrefix);
                return IsValidAlphaQualitySuffix(suffix) ? AlphaQuality : Unknown;
            }

            if (branchName == _options.DetachedHeadName)
            {
                return AlphaQuality;
            }

            return BranchCategory.Unknown;
        }

        private string GetBranchSuffix(string branchName, string prefix) =>
            branchName.Substring(prefix.Length);

        private bool IsValidReleaseCandidateSuffix(string suffix) =>
            SemanticVersion.TryParse(suffix, out SemanticVersion version) && !version.IsPrerelease;

        private bool IsValidAlphaQualitySuffix(string suffix) => AlphaQualitySuffixPattern.IsMatch(suffix);
    }
}
