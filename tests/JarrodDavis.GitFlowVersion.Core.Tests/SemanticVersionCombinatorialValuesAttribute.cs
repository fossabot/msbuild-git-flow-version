using System.Linq;
using NuGet.Versioning;
using Xunit;

namespace JarrodDavis.GitFlowVersion.Core.Tests
{
    public class SemanticVersionCombinatorialValuesAttribute : CombinatorialValuesAttribute
    {
        private static SemanticVersion[] ParseVersionStrings(string[] values)
        {
            return values.Select(value => value == null ? null : SemanticVersion.Parse(value))
                         .ToArray();
        }

        public SemanticVersionCombinatorialValuesAttribute(params string[] values)
            : base(ParseVersionStrings(values))
        {
        }
    }
}
