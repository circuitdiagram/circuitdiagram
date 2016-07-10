using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace CircuitDiagram.IO.Descriptions.Xml
{
    static class XElementExtensions
    {
        public static bool GetAttribute(this XElement element, string name, LoadContext lc, out string set)
        {
            IXmlLineInfo line = element as IXmlLineInfo;

            var attr = element.Attribute(name);
            if (attr == null)
            {
                lc.Errors.Add(new LoadError(lc.FileName, line.LineNumber, line.LinePosition, LoadErrorCategory.Error,
                    String.Format("Missing attribute '{0}' for <{1}> tag", name, element.Name.LocalName)));
                set = null;
                return false;
            }
            else
            {
                set = attr.Value;
                return true;
            }
        }
    }
}
