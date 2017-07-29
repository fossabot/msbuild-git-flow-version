namespace JarrodDavis.GitFlowVersion.Core
{
    public interface IBranchCategoryMapper
    {
        (BranchCategory Category, string Suffix) MapBranchName(string branchName);
    }
}
