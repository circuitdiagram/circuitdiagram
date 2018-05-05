using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Logging
{
    class ErrorCheckingLogger : IXmlLoadLogger
    {
        private readonly IXmlLoadLogger underlying;

        public ErrorCheckingLogger(IXmlLoadLogger underlying)
        {
            this.underlying = underlying;
        }

        public bool HasErrors { get; private set; }

        public void Log(LogLevel level, FileRange position, string message, Exception innerException)
        {
            if (level >= LogLevel.Error)
                HasErrors = true;

            underlying.Log(level, position, message, innerException);
        }
    }
}
