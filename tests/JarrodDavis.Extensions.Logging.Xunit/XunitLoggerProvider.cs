using System;

using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace JarrodDavis.Extensions.Logging.Xunit
{
    internal class XunitLoggerProvider : ILoggerProvider
    {
        private LogLevel _minimumLogLevel;
        private ITestOutputHelper _output;
        private IXunitLoggerScopeStack _scopeStack;

        public XunitLoggerProvider(LogLevel minimumLogLevel,
                                   ITestOutputHelper output,
                                   IXunitLoggerScopeStack scopeStack)
        {
            _minimumLogLevel = minimumLogLevel;
            _output = output;
            _scopeStack = scopeStack;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XunitLogger(_minimumLogLevel, categoryName, _output, _scopeStack);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
