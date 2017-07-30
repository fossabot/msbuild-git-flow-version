using NuGet.Versioning;

namespace JarrodDavis.GitFlowVersion.Core.Contracts
{
    public interface IVersionResolver
    {
        SemanticVersion ResolveVersion(VersionResolutionRequest request);
    }
}
