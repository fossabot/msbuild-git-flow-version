using System;
using System.Linq;

using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace JarrodDavis.Extensions.Logging.Xunit
{
    internal class XunitLogger : ILogger
    {
        private static readonly int LogLevelPadding = Enum.GetNames(typeof(LogLevel))
                                                          .OrderByDescending(name => name.Length)
                                                          .Select(name => name.Length)
                                                          .First();

        private LogLevel _minimumLogLevel;
        private string _name;
        private ITestOutputHelper _output;
        private IXunitLoggerScopeStack _scopeStack;

        public XunitLogger(LogLevel minimumLogLevel,
                           string name,
                           ITestOutputHelper output,
                           IXunitLoggerScopeStack scopeStack)
        {
            _minimumLogLevel = minimumLogLevel;
            _name = name;
            _output = output;
            _scopeStack = scopeStack;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _scopeStack.Push(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _minimumLogLevel;
        }

        public void Log<TState>(LogLevel logLevel,
                                EventId eventId,
                                TState state,
                                Exception exception,
                                Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var paddedLevel = PadLogLevel(logLevel);
            var paddedEmpty = PadEmpty();

            _output.WriteLine($"{paddedLevel}: {_name}[{eventId}]");
            _output.WriteLine($"{paddedEmpty}  {formatter(state, exception)}");
        }

        private string PadLogLevel(LogLevel logLevel) => logLevel.ToString().PadLeft(LogLevelPadding);

        private string PadEmpty() => "".PadLeft(LogLevelPadding);
    }
}
