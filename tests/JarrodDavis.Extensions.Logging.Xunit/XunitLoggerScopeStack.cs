using System;
using System.Collections.Generic;
using System.Threading;

namespace JarrodDavis.Extensions.Logging.Xunit
{
    internal class XunitLoggerScopeStack : IXunitLoggerScopeStack
    {
        private AsyncLocal<Stack<object>> _scopes;

        public XunitLoggerScopeStack()
        {
            _scopes = new AsyncLocal<Stack<object>>();
        }

        public IDisposable Push<TState>(TState state)
        {
            GetStack().Push(state);
            return new XunitScopePopper(Pop);
        }

        private Stack<object> GetStack()
        {
            var stack = _scopes.Value;

            if (stack is null)
            {
                stack = new Stack<object>();
                _scopes.Value = stack;
            }

            return stack;
        }

        private void Pop()
        {
            var stack = GetStack();
            if (stack.Count == 0)
            {
                return;
            }

            try
            {
                stack.Pop();
            }
            catch (InvalidOperationException)
            {
                // Do Nothing
            }
        }
    }
}
