using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Logging
{
    public class NullXmlLoadLogger : IXmlLoadLogger
    {
        public void Log(LogLevel level, FileRange position, string message, Exception innerException)
        {
            // Do nothing
        }
    }
}
