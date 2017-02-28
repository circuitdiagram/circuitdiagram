using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitDiagram.Logging
{
    public interface ILog
    {
        void Info(object message);
        void Error(object message, Exception exception);
    }
}
