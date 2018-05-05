using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Logging
{
    public class XmlLoadLogger : IXmlLoadLogger
    {
        private readonly ILogger logger;
        private readonly string fileName;

        public XmlLoadLogger(ILogger logger, string fileName)
        {
            this.logger = logger;
            this.fileName = fileName.Replace(Environment.CurrentDirectory, "."); ;
        }

        public void Log(LogLevel level, FileRange position, string message, Exception innerException)
        {
            var formattedMessage = $"{level.ToString().ToUpperInvariant()} {fileName}({position.StartLine},{position.StartCol}:{position.EndLine},{position.EndCol}): {message}";
            logger.Log(level, new EventId(), (object)null, innerException, (state, ex) => formattedMessage);
        }
    }
}
