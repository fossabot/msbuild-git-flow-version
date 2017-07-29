using System;
using Microsoft.Extensions.Options;

namespace JarrodDavis.GitFlowVersion.Core
{
    public class BranchCategoryMapper : IBranchCategoryMapper
    {
        private BranchCategoryMappingOptions _options;

        public BranchCategoryMapper(IOptions<BranchCategoryMappingOptions> options)
        {
            _options = options.Value;
        }

        public BranchCategory MapBranchName(string branchName)
        {
            return BranchCategory.Unknown;
        }
    }
}
