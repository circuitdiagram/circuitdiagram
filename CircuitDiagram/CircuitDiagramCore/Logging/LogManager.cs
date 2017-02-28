using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Logging
{
    public static class LogManager
    {
        private static Func<Type, ILog> LoggerFactory;

        public static void Initialize(Func<Type, ILog> loggerFactory)
        {
            LoggerFactory = loggerFactory;
        }

        public static ILog GetLogger(Type t) => LoggerFactory(t);
    }
}
