using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.Logging
{
    public static class LogManager
    {
        public static ILoggerFactory LoggerFactory { get; } = new LoggerFactory();

        public static ILogger GetLogger<T>() => LoggerFactory.CreateLogger<T>();
    }
}
