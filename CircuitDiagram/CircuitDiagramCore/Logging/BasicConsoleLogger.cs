using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.Logging
{
    public class BasicConsoleLogger : ILoggerProvider
    {
        private readonly LogLevel level;

        public BasicConsoleLogger(LogLevel level)
        {
            this.level = level;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new ConsoleLogger(level);
        }
        
        public void Dispose()
        {
            // Do nothing
        }

        private class ConsoleLogger : ILogger, IDisposable
        {
            private readonly LogLevel level;

            public ConsoleLogger(LogLevel level)
            {
                this.level = level;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (IsEnabled(logLevel))
                    Console.WriteLine(formatter(state, exception));
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return logLevel >= level;
            }

            IDisposable ILogger.BeginScope<TState>(TState state)
            {
                return this;
            }

            void IDisposable.Dispose()
            {
                // Do nothing
            }
        }
    }
}
