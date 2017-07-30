namespace JarrodDavis.GitFlowVersion.Core.Contracts
{
    public interface IVersionResolutionRequestValidator
    {
        void ValidateRequest(VersionResolutionRequest request);
    }
}
