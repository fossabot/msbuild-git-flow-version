using NuGet.Versioning;

namespace JarrodDavis.GitFlowVersion.Core
{
    public interface IVersionResolver
    {
        SemanticVersion ResolveVersion(VersionResolutionRequest request);
    }
}
