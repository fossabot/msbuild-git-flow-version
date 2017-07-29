using System;

namespace JarrodDavis.GitFlowVersion.Core
{
    public class BranchCategoryMapper : IBranchCategoryMapper
    {
        public BranchCategory MapBranchName(string branchName)
        {
            return BranchCategory.Unknown;
        }
    }
}
