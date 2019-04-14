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
            this.fileName = fileName?.Replace(Environment.CurrentDirectory, ".") ?? "File.xml";
        }

        public void Log(LogLevel level, FileRange position, string message, Exception innerException)
        {
            logger.Log(level, new EventId(), (object)null, innerException, (state, ex) =>
            {
                var builder = new StringBuilder();
                builder.Append($"{level.ToString().ToUpperInvariant()} {fileName}({position.StartLine},{position.StartCol}:{position.EndLine},{position.EndCol}): {message}");

                if (ex != null)
                {
                    builder.AppendLine();
                    builder.Append(ex);
                }

                return builder.ToString();
            });
        }
    }
}
