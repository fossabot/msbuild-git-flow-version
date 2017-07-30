using NuGet.Versioning;

namespace JarrodDavis.GitFlowVersion.Core.Contracts
{
    public struct VersionResolutionRequest
    {
        public string CurrentBranchName { get; set; }

        public string BaseBranchName { get; set; }

        public SemanticVersion MostRecentStableReleaseVersion { get; set; }
    }
}
