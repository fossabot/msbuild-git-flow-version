using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace JarrodDavis.GitFlowVersion.Core
{
    public partial class BranchCategoryMapper : IBranchCategoryMapper
    {
        private IDictionary<string, (BranchCategory, string)> _simpleMatchers;
        private SuffixMatcher[] _suffixMatchers;

        public BranchCategoryMapper(IOptions<BranchCategoryMappingOptions> options)
        {
            var names = options.Value;

            _simpleMatchers = new Dictionary<string, (BranchCategory, string)>
            {
                { names.StableBranchName, (BranchCategory.Stable, null) },
                { names.BetaQualityBranchName, (BranchCategory.BetaQuality, null) },
                { names.DetachedHeadName, (BranchCategory.AlphaQuality, names.DetachedHeadName) }
            };

            _suffixMatchers = new SuffixMatcher[]
            {
                new ReleaseCandidateSuffixMatcher(names.ReleaseBranchNamePrefix),
                new ReleaseCandidateSuffixMatcher(names.HotfixBranchNamePrefix),
                new AlphaQualitySuffixMatcher(names.FeatureBranchNamePrefix),
                new AlphaQualitySuffixMatcher(names.BugfixBranchNamePrefix)
            };
        }

        public (BranchCategory Category, string Suffix) MapBranchName(string branchName)
        {
            if (branchName is null)
            {
                throw new ArgumentNullException(nameof(branchName));
            }

            if (string.IsNullOrWhiteSpace(branchName))
            {
                throw new ArgumentException("Branch name cannot be empty or whitespace", nameof(branchName));
            }

            return _simpleMatchers.TryGetValue(branchName, out (BranchCategory, string) match)
                ? match
                : _suffixMatchers.Select(matcher => matcher.MatchSuffix(branchName))
                                 .FirstOrDefault(
                                     possibleMatch => possibleMatch.Category != BranchCategory.Unknown
                                 );
        }
    }
}
