using System;

namespace JarrodDavis.Extensions.Logging.Xunit
{
    internal class XunitScopePopper : IDisposable
    {
        private Action _popScope;

        public XunitScopePopper(Action popScope)
        {
            _popScope = popScope;
        }

        public void Dispose()
        {
            _popScope();
        }
    }
}
