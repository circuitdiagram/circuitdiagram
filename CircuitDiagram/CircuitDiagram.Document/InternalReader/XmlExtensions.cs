// Circuit Diagram http://www.circuit-diagram.org/
// 
// Copyright (C) 2016  Samuel Fisher
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CircuitDiagram.Circuit;
using CircuitDiagram.Document.ReaderErrors;
using CircuitDiagram.Primitives;

namespace CircuitDiagram.Document.InternalReader
{
    static class XmlExtensions
    {
        public static int? GetIntAttribute(this XElement element, XName attribute, ReaderContext context)
        {
            var attr = element.Attribute(attribute);
            if (attr == null)
                return null;

            int val;
            if (int.TryParse(attr.Value, out val))
                return val;

            context.Log(ReaderErrorCodes.UnableToParseValueAsInt, element, attr.Value);
            return null;
        }

        public static Guid? GetGuidAttribute(this XElement element, XName attribute, ReaderContext context)
        {
            var attr = element.Attribute(attribute);
            if (attr == null)
                return null;

            Guid guid;
            if (Guid.TryParse(attr.Value, out guid))
                return guid;

            context.Log(ReaderErrorCodes.UnableToParseValueAsGuid, element, attr.Value);
            return null;
        }

        public static bool? GetBoolAttribute(this XElement element, XName attribute, ReaderContext context)
        {
            var attr = element.Attribute(attribute);
            if (attr == null)
                return null;

            bool val;
            if (bool.TryParse(attr.Value, out val))
                return val;

            context.Log(ReaderErrorCodes.UnableToParseValueAsBoolean, element, attr.Value);
            return null;
        }

        public static string GetCollectionItemAttribute(this XElement element,
                                                        XName attribute,
                                                        ReaderContext context)
        {
            return element.Attribute(attribute)?.Value;
        }

        public static string GetComponentNameAttribute(this XElement element,
                                                              XName attribute,
                                                              ReaderContext context)
        {
            return element.Attribute(attribute)?.Value;
        }

        public static Orientation? GetComponentOrientationAttribute(this XElement element,
                                                                    XName attribute,
                                                                    ReaderContext context)
        {
            var attr = element.Attribute(attribute);
            if (attr == null)
                return null;

            switch (attr.Value)
            {
                case "h":
                    return Orientation.Horizontal;
                case "v":
                    return Orientation.Vertical;
                default:
                {
                    context.Log(ReaderErrorCodes.UnableToParseValueAsOrientation, element, attr.Value);
                    return null;
                }
            }
        }
    }
}
