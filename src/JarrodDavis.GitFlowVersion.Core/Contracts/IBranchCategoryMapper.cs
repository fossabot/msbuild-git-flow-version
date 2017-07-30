namespace JarrodDavis.GitFlowVersion.Core.Contracts
{
    public interface IBranchCategoryMapper
    {
        (BranchCategory Category, string Suffix) MapBranchName(string branchName);
    }
}
