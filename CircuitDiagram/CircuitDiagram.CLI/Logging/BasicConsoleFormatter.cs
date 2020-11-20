using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.CLI.Logging
{
    class BasicConsoleFormatter : ConsoleFormatter
    {
        public BasicConsoleFormatter() : base(nameof(BasicConsoleFormatter))
        {
        }

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
        {
            textWriter.WriteLine(logEntry.Formatter(logEntry.State, logEntry.Exception));
        }
    }

    class BasicConsoleFormatterOptions : ConsoleFormatterOptions
    {
    }
}
