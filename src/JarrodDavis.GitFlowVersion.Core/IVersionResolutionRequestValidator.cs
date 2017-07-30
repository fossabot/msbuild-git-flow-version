namespace JarrodDavis.GitFlowVersion.Core
{
    public interface IVersionResolutionRequestValidator
    {
        void ValidateRequest(VersionResolutionRequest request);
    }
}
