using System;

namespace JarrodDavis.Extensions.Logging.Xunit
{
    internal interface IXunitLoggerScopeStack
    {
        IDisposable Push<TState>(TState state);
    }
}
