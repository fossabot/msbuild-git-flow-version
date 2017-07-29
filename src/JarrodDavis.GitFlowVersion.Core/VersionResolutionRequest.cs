using NuGet.Versioning;

namespace JarrodDavis.GitFlowVersion.Core
{
    public struct VersionResolutionRequest
    {
        public string CurrentBranchName { get; set; }

        public string BaseBranchName { get; set; }

        public SemanticVersion MostRecentStableReleaseVersion { get; set; }

        public int CommitsSincePreviousStableRelease { get; set; }
    }
}
