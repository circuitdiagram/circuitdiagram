using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Logging
{
    class Log4NetLogger : ILog
    {
        private readonly log4net.ILog log;

        public Log4NetLogger(log4net.ILog log)
        {
            this.log = log;
        }

        public void Info(object message)
        {
            log.Info(message);
        }

        public void Error(object message, Exception exception)
        {
            log.Error(message, exception);
        }
    }
}
