using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace JarrodDavis.Extensions.Logging.Xunit
{
    public static class XunitLoggerFactoryExtensions
    {
        public static ILoggerFactory AddXunit(this ILoggerFactory factory,
                                              LogLevel minimumLogLevel,
                                              ITestOutputHelper output)
        {
            factory.AddProvider(new XunitLoggerProvider(minimumLogLevel,
                                                        output,
                                                        new XunitLoggerScopeStack()));
            return factory;
        }
    }
}
