// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2018  Samuel Fisher
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace CircuitDiagram.TypeDescriptionIO.Xml.Logging
{
    public interface IXmlLoadLogger
    {
        void Log(LogLevel level, FileRange position, string message, Exception innerException);
    }

    public static class XmlLoadLoggerExtensions
    {
        public static void Log(this IXmlLoadLogger logger, LogLevel level, XElement element, string message)
        {
            logger.Log(level, element.GetFileRange(), message, null);
        }
        
        public static void Log(this IXmlLoadLogger logger, LogLevel level, XAttribute attribute, string message)
        {
            logger.Log(level, attribute.GetFileRange(), message, null);
        }

        public static void LogWarning(this IXmlLoadLogger logger, XElement element, string message)
        {
            logger.Log(LogLevel.Warning, element, message);
        }

        public static void LogError(this IXmlLoadLogger logger, XElement element, string message)
        {
            logger.Log(LogLevel.Error, element, message);
        }

        public static void LogWarning(this IXmlLoadLogger logger, XAttribute attribute, string message)
        {
            logger.Log(LogLevel.Warning, attribute, message);
        }

        public static void LogError(this IXmlLoadLogger logger, XAttribute attribute, string message)
        {
            logger.Log(LogLevel.Error, attribute, message);
        }
    }
}
