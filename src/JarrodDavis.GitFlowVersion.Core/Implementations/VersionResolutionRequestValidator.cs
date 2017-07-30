using System;

using JarrodDavis.GitFlowVersion.Core.Contracts;

namespace JarrodDavis.GitFlowVersion.Core.Implementations
{
    internal class VersionResolutionRequestValidator : IVersionResolutionRequestValidator
    {
        public void ValidateRequest(VersionResolutionRequest request)
        {
            if (request.CurrentBranchName is null)
            {
                throw new ArgumentNullException(nameof(request.CurrentBranchName));
            }

            if (request.BaseBranchName is null)
            {
                throw new ArgumentNullException(nameof(request.BaseBranchName));
            }

            if (request.MostRecentStableReleaseVersion is null)
            {
                throw new ArgumentNullException(nameof(request.MostRecentStableReleaseVersion));
            }

            if (string.IsNullOrWhiteSpace(request.CurrentBranchName))
            {
                throw new ArgumentException("Current branch name cannot be empty or whitespace",
                    nameof(request.CurrentBranchName));
            }

            if (string.IsNullOrWhiteSpace(request.BaseBranchName))
            {
                throw new ArgumentException("Base branch name cannot be empty or whitespace",
                    nameof(request.BaseBranchName));
            }

            if (request.MostRecentStableReleaseVersion.IsPrerelease)
            {
                throw new ArgumentException("Stable release version cannot be a prerelease",
                    nameof(request.MostRecentStableReleaseVersion));
            }
        }
    }
}
