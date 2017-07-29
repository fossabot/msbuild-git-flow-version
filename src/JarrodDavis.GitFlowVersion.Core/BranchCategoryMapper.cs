using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace JarrodDavis.GitFlowVersion.Core
{
    public partial class BranchCategoryMapper : IBranchCategoryMapper
    {
        private IDictionary<string, BranchCategory> _simpleMatchers;
        private SuffixMatcher[] _suffixMatchers;

        public BranchCategoryMapper(IOptions<BranchCategoryMappingOptions> options)
        {
            var names = options.Value;

            _simpleMatchers = new Dictionary<string, BranchCategory>
            {
                { names.StableBranchName, BranchCategory.Stable },
                { names.BetaQualityBranchName, BranchCategory.BetaQuality },
                { names.DetachedHeadName, BranchCategory.AlphaQuality }
            };

            _suffixMatchers = new SuffixMatcher[]
            {
                new ReleaseCandidateSuffixMatcher(names.ReleaseBranchNamePrefix),
                new ReleaseCandidateSuffixMatcher(names.HotfixBranchNamePrefix),
                new AlphaQualitySuffixMatcher(names.FeatureBranchNamePrefix),
                new AlphaQualitySuffixMatcher(names.BugfixBranchNamePrefix)
            };
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

            return _simpleMatchers.TryGetValue(branchName, out BranchCategory category)
                ? category
                : _suffixMatchers.FirstOrDefault(
                    matcher => matcher.IsValidBranchName(branchName)
                )?.Category ?? BranchCategory.Unknown;
        }
    }
}
