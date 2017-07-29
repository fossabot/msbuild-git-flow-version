using System.Text.RegularExpressions;
using NuGet.Versioning;

namespace JarrodDavis.GitFlowVersion.Core
{
    public partial class BranchCategoryMapper : IBranchCategoryMapper
    {
        private abstract class SuffixMatcher
        {
            private string _prefix;

            public abstract BranchCategory Category { get; }

            public SuffixMatcher(string prefix)
            {
                _prefix = prefix;
            }

            public (BranchCategory Category, string Suffix) MatchSuffix(string branchName)
            {
                if (!branchName.StartsWith(_prefix))
                {
                    return (BranchCategory.Unknown, null);
                }

                var suffix = branchName.Substring(_prefix.Length);
                return IsValidSuffix(suffix) ? (Category, suffix) : (BranchCategory.Unknown, null);
            }

            protected abstract bool IsValidSuffix(string suffix);
        }

        private class ReleaseCandidateSuffixMatcher : SuffixMatcher
        {
            public override BranchCategory Category => BranchCategory.ReleaseCandidate;

            public ReleaseCandidateSuffixMatcher(string prefix) : base(prefix)
            {
            }

            protected override bool IsValidSuffix(string suffix) =>
                SemanticVersion.TryParse(suffix, out SemanticVersion version) && !version.IsPrerelease;
        }

        private class AlphaQualitySuffixMatcher : SuffixMatcher
        {
            // Only alphanumeric (and the hyphen) characters are allowed
            private static readonly Regex AlphaQualitySuffixPattern = new Regex("^[a-zA-Z0-9-]+$");

            public override BranchCategory Category => BranchCategory.AlphaQuality;

            public AlphaQualitySuffixMatcher(string prefix) : base(prefix)
            {
            }

            protected override bool IsValidSuffix(string suffix) =>
                AlphaQualitySuffixPattern.IsMatch(suffix);
        }
    }
}
