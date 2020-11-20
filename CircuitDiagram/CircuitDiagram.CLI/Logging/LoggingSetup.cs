using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.CLI.Logging
{
    static class LoggingSetup
    {
        public static ILoggingBuilder SetupLogging(this ILoggingBuilder builder, bool verbose, bool silent)
        {
            builder.AddConsole(x => x.FormatterName = "BasicConsoleFormatter").AddConsoleFormatter<BasicConsoleFormatter, BasicConsoleFormatterOptions>().SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Information);
            return builder;
        }
    }
}
