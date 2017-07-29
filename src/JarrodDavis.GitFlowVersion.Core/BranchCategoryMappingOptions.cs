namespace JarrodDavis.GitFlowVersion.Core
{
    public class BranchCategoryMappingOptions
    {
        public string StableBranchName { get; set; }
        public string BetaQualityBranchName { get; set; }
        public string FeatureBranchNamePrefix { get; set; }
        public string BugfixBranchNamePrefix { get; set; }
        public string ReleaseBranchNamePrefix { get; set; }
        public string HotfixBranchNamePrefix { get; set; }
        public string DetachedHeadName { get; set; }
    }
}
